using CustodialWallet.Application.Wallets.Commands;
using CustodialWallet.Domain.Entities;
using CustodialWallet.Domain.Exceptions;
using CustodialWallet.Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace CustodialWallets.UnitTests.Wallets;

public class WithdrawFundsCommandHandlerTests
{
    private readonly Mock<IWalletRepository> _walletRepoMock = new();
    private readonly Mock<IValidator<WithdrawFundsCommand>> _validatorMock = new();
    private readonly Mock<ILogger<WithdrawFundsCommandHandler>> _loggerMock = new();
    private readonly WithdrawFundsCommandHandler _handler;

    public WithdrawFundsCommandHandlerTests()
    {
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<WithdrawFundsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        
        _handler = new WithdrawFundsCommandHandler(
            _walletRepoMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenWalletNotFound()
    {
        // Arrange
        var command = new WithdrawFundsCommand(Guid.NewGuid(), "BTC", 50, "addr1");

        _walletRepoMock
            .Setup(x => x.GetByUserAndCurrency(
                command.UserId,
                command.Currency,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenInsufficientFunds()
    {
        // Arrange
        var command = new WithdrawFundsCommand(Guid.NewGuid(), "BTC", 150, "addr1");
        var wallet = CreateWalletWithBalance(100);

        _walletRepoMock
            .Setup(x => x.GetByUserAndCurrency(
                command.UserId,
                command.Currency,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InsufficientFundsException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains($"Текущий баланс: 100 BTC", ex.Message);
    }

    [Fact]
    public async Task Handle_ShouldWithdrawFunds_WhenValid()
    {
        // Arrange
        var command = new WithdrawFundsCommand(Guid.NewGuid(), "BTC", 50, "addr1");
        var wallet = CreateWalletWithBalance(100);

        _walletRepoMock
            .Setup(x => x.GetByUserAndCurrency(
                command.UserId,
                command.Currency,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(50, wallet.Balance);
        _walletRepoMock.Verify(
            x => x.Update(wallet, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogWithdrawal()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new WithdrawFundsCommand(userId, "BTC", 50, "addr1");
        var wallet = CreateWalletWithBalance(100);

        _walletRepoMock
            .Setup(x => x.GetByUserAndCurrency(
                command.UserId,
                command.Currency,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!
                    .Contains($"Вывод от пользователя {userId} на сумму 50 BTC на адрес addr1")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new WithdrawFundsCommand(userId, "BTC", 50, "addr1");
        var wallet = CreateWalletWithBalance(100);

        _walletRepoMock
            .Setup(x => x.GetByUserAndCurrency(
                command.UserId,
                command.Currency,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(userId, result.UserId);
        Assert.Equal("BTC", result.Currency);
        Assert.Equal(50, result.Amount);
        Assert.Equal(50, result.NewBalance);
    }

    private static Wallet CreateWalletWithBalance(decimal balance)
    {
        var wallet = new Wallet();
        wallet.Deposit(balance);
        
        return wallet;
    }
}