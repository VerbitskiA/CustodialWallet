using CustodialWallet.Application.Users.Queries;
using CustodialWallet.Domain.Entities;
using CustodialWallet.Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace CustodialWallets.UnitTests.Users;

public class GetUserBalanceQueryHandlerTests
{
    private readonly Mock<IWalletRepository> _walletRepositoryMock = new();
    private readonly Mock<IValidator<GetUserBalanceQuery>> _validatorMock = new();
    private readonly GetUserBalanceQueryHandler _handler;

    public GetUserBalanceQueryHandlerTests()
    {
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<GetUserBalanceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _handler = new GetUserBalanceQueryHandler(_walletRepositoryMock.Object, _validatorMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllWallets_WhenCurrencyNotSpecified()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserBalanceQuery(userId);

        var wallets = new List<Wallet>
        {
            new() { Currency = "BTC" },
            new() { Currency = "ETH" }
        };

        _walletRepositoryMock
            .Setup(r => r.GetUserWallets(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallets);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(userId, result.UserId);
        Assert.Equal(2, result.Wallets.Count);
        Assert.Contains(result.Wallets, w => w.Currency == "BTC");
        Assert.Contains(result.Wallets, w => w.Currency == "ETH");
    }

    [Fact]
    public async Task Handle_ShouldReturnSingleWallet_WhenCurrencySpecified()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currency = "USDT";
        var query = new GetUserBalanceQuery(userId, currency);

        var wallet = new Wallet { Currency = currency };

        _walletRepositoryMock
            .Setup(r => r.GetByUserAndCurrency(userId, currency, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(userId, result.UserId);
        Assert.Single(result.Wallets);
        Assert.Equal(currency, result.Wallets[0].Currency);
        Assert.Equal(0, result.Wallets[0].Balance);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenWalletNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currency = "ETH";
        var query = new GetUserBalanceQuery(userId, currency);

        _walletRepositoryMock
            .Setup(r => r.GetByUserAndCurrency(userId, currency, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(userId, result.UserId);
        Assert.Empty(result.Wallets);
    }
}