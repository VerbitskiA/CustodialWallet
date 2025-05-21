using CustodialWallet.Application.Settings;
using CustodialWallet.Application.Wallets.Commands;
using CustodialWallet.Application.Wallets.Commands.Validators;
using CustodialWallet.Domain.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Options;
using Moq;

namespace CustodialWallets.UnitTests.Wallets.Validators;

public class DepositFundsCommandValidatorTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly DepositFundsCommandValidator _validator;
    private readonly Mock<IOptionsSnapshot<CurrencySettings>> _currencySettingsMock = new();

    public DepositFundsCommandValidatorTests()
    {
        _currencySettingsMock.Setup(x => x.Value).Returns(new CurrencySettings()
        {
            AllowedCurrencies = new[] { "BTC", "ETH", "SOL" }
        });
        
        _validator = new DepositFundsCommandValidator(_userRepositoryMock.Object, _currencySettingsMock.Object);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new DepositFundsCommand(Guid.Empty, "BTC", 100);

        // Act & Assert
        var result = await _validator.TestValidateAsync(command);
        
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage("User Id обязателен.");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DepositFundsCommand(userId, "BTC", 100);
        
        _userRepositoryMock
            .Setup(x => x.Exists(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var result = await _validator.TestValidateAsync(command);
        
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage($"Пользователь с Id {userId} не найден.");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenCurrencyIsEmpty()
    {
        // Arrange
        var command = new DepositFundsCommand(Guid.NewGuid(), "", 100);

        // Act & Assert
        var result = await _validator.TestValidateAsync(command);
        
        result.ShouldHaveValidationErrorFor(x => x.Currency)
              .WithErrorMessage("Валюта обязательна.");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenCurrencyIsNotAllowed()
    {
        // Arrange
        var command = new DepositFundsCommand(Guid.NewGuid(), "XRP", 100);

        // Act & Assert
        var result = await _validator.TestValidateAsync(command);
        
        result.ShouldHaveValidationErrorFor(x => x.Currency)
              .WithErrorMessage("Недопустимая валюта.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task Validate_ShouldFail_WhenAmountIsInvalid(decimal amount)
    {
        // Arrange
        var command = new DepositFundsCommand(Guid.NewGuid(), "BTC", amount);

        // Act & Assert
        var result = await _validator.TestValidateAsync(command);
        
        result.ShouldHaveValidationErrorFor(x => x.Amount)
              .WithErrorMessage("Сумма должна быть больше 0.");
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DepositFundsCommand(userId, "BTC", 100);
        
        _userRepositoryMock
            .Setup(x => x.Exists(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var result = await _validator.TestValidateAsync(command);
        
        result.ShouldNotHaveAnyValidationErrors();
    }
}