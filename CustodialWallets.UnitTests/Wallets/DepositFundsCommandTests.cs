using CustodialWallet.Application.Wallets.Commands;
using CustodialWallet.Domain.Entities;
using CustodialWallet.Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace CustodialWallets.UnitTests.Wallets;

public class DepositFundsCommandHandlerTests
{
    private readonly Mock<IWalletRepository> _walletRepositoryMock = new();
    private readonly Mock<IValidator<DepositFundsCommand>> _validatorMock = new();
    private readonly Mock<ILogger<DepositFundsCommand>> _loggerMock = new();
    private readonly DepositFundsCommandHandler _handler;

    public DepositFundsCommandHandlerTests()
    {
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<DepositFundsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        
        _handler = new DepositFundsCommandHandler(
            _walletRepositoryMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateNewWallet_WhenNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DepositFundsCommand(userId, "BTC", 100);
        
        _walletRepositoryMock
            .Setup(x => x.GetByUserAndCurrency(userId, "BTC", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _walletRepositoryMock.Verify(
            x => x.Add(It.Is<Wallet>(w => 
                w.UserId == userId && 
                w.Currency == "BTC"), 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDepositToExistingWallet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DepositFundsCommand(userId, "BTC", 100);
        var existingWallet = new Wallet { UserId = userId, Currency = "BTC" };
        
        _walletRepositoryMock
            .Setup(x => x.GetByUserAndCurrency(userId, "BTC", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingWallet);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(100, existingWallet.Balance);
        _walletRepositoryMock.Verify(
            x => x.Update(existingWallet, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogDepositInformation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DepositFundsCommand(userId, "BTC", 100);
        
        _walletRepositoryMock
            .Setup(x => x.GetByUserAndCurrency(userId, "BTC", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Wallet());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!
                    .Contains($"Совершен новый депозит от пользователя с Id {userId} на сумму 100 BTC")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DepositFundsCommand(userId, "BTC", 100);
        var wallet = new Wallet { UserId = userId};
        
        _walletRepositoryMock
            .Setup(x => x.GetByUserAndCurrency(userId, "BTC", It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(userId, result.UserId);
        Assert.Equal(100, result.NewBalance);
    }
}