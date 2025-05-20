using CustodialWallet.Application.Users.Queries;
using CustodialWallet.Application.Users.Queries.Validators;
using CustodialWallet.Domain.Interfaces;
using FluentValidation.TestHelper;
using Moq;

namespace CustodialWallets.UnitTests.Users.Validators;

public class GetUserBalanceQueryValidatorTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly GetUserBalanceQueryValidator _validator;

    public GetUserBalanceQueryValidatorTests()
    {
        _validator = new GetUserBalanceQueryValidator(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenUserIdIsEmpty()
    {
        // Arrange
        var query = new GetUserBalanceQuery(Guid.Empty);

        // Act & Assert
        var result = await _validator.TestValidateAsync(query);
        
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage("User Id обязателен.");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserBalanceQuery(userId);
        
        _userRepositoryMock
            .Setup(x => x.Exists(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var result = await _validator.TestValidateAsync(query);
        
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage($"Пользователь с Id {userId} не найден.");
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserBalanceQuery(userId);
        
        _userRepositoryMock
            .Setup(x => x.Exists(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var result = await _validator.TestValidateAsync(query);
        
        result.ShouldNotHaveAnyValidationErrors();
    }
}