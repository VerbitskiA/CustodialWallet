using CustodialWallet.Application.Settings;
using CustodialWallet.Application.Wallets.Commands;
using CustodialWallet.Application.Wallets.Commands.Validators;
using CustodialWallet.Domain.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Options;
using Moq;

namespace CustodialWallets.UnitTests.Wallets.Validators
{
    public class WithdrawFundsCommandValidatorTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly WithdrawFundsCommandValidator _validator;
        private readonly Mock<IOptionsSnapshot<CurrencySettings>> _currencySettingsMock = new();

        public WithdrawFundsCommandValidatorTests()
        {
            _currencySettingsMock.Setup(x => x.Value).Returns(new CurrencySettings()
            {
                AllowedCurrencies = new[] { "BTC", "ETH", "SOL" }
            });
            
            _userRepositoryMock = new Mock<IUserRepository>();
            
            _validator = new WithdrawFundsCommandValidator(_currencySettingsMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task Validate_WhenAllFieldsAreValid_ShouldNotHaveValidationErrors()
        {
            // Arrange
            var command = new WithdrawFundsCommand(Guid.NewGuid(), "BTC", 100, "valid-address");

            _userRepositoryMock.Setup(x => x.Exists(command.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WhenUserIdIsEmpty_ShouldHaveValidationError()
        {
            // Arrange
            var command = new WithdrawFundsCommand(Guid.Empty, "BTC", 100, "valid-address");

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                .WithErrorMessage("User ID обязателен.");
        }

        [Fact]
        public async Task Validate_WhenUserDoesNotExist_ShouldHaveValidationError()
        {
            // Arrange
            var command = new WithdrawFundsCommand(Guid.NewGuid(), "BTC", 100, "valid-address");

            _userRepositoryMock.Setup(x => x.Exists(command.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                .WithErrorMessage($"Пользователь с Id {command.UserId} не найден.");
        }

        [Fact]
        public async Task Validate_WhenCurrencyIsEmpty_ShouldHaveValidationError()
        {
            // Arrange
            var command = new WithdrawFundsCommand(Guid.NewGuid(), string.Empty, 100, "valid-address");

            _userRepositoryMock.Setup(x => x.Exists(command.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Currency)
                .WithErrorMessage("Валюта обязательна.");
        }

        [Fact]
        public async Task Validate_WhenCurrencyIsNotAllowed_ShouldHaveValidationError()
        {
            // Arrange
            var command = new WithdrawFundsCommand(Guid.NewGuid(), "PEPE", 100, "valid-address");

            _userRepositoryMock.Setup(x => x.Exists(command.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Currency)
                .WithErrorMessage("Недопустимая валюта. Допустимые: BTC, ETH, SOL.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public async Task Validate_WhenAmountIsZeroOrNegative_ShouldHaveValidationError(decimal amount)
        {
            // Arrange
            var command = new WithdrawFundsCommand(Guid.NewGuid(), "PEPE", amount, "valid-address");

            _userRepositoryMock.Setup(x => x.Exists(command.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Amount)
                .WithErrorMessage("Сумма должна быть больше 0.");
        }

        [Fact]
        public async Task Validate_WhenDestinationAddressIsEmpty_ShouldHaveValidationError()
        {
            // Arrange
            var command = new WithdrawFundsCommand(Guid.NewGuid(), "PEPE", 100, string.Empty);

            _userRepositoryMock.Setup(x => x.Exists(command.UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DestinationAddress)
                .WithErrorMessage("Адрес кошелька обязателен.");
        }
    }
}