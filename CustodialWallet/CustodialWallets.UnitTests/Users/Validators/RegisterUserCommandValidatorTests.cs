using CustodialWallet.Application.Users.Commands;
using CustodialWallet.Application.Users.Commands.Validators;
using FluentValidation.TestHelper;

namespace CustodialWallets.UnitTests.Users.Validators;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name+tag@domain.co.uk")]
    [InlineData("firstname.lastname@example.com")]
    public void Validate_ShouldPass_WhenEmailIsValid(string email)
    {
        // Arrange
        var command = new RegisterUserCommand(email);

        // Act & Assert
        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenEmailIsEmpty()
    {
        // Arrange
        var command = new RegisterUserCommand("");

        // Act & Assert
        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email обязателен.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    public void Validate_ShouldFail_WhenEmailIsInvalid(string email)
    {
        // Arrange
        var command = new RegisterUserCommand(email);

        // Act & Assert
        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Некорректный email.");
    }
}