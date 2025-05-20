using CustodialWallet.Application.Users.Commands;
using CustodialWallet.Domain.Entities;
using CustodialWallet.Domain.Exceptions;
using CustodialWallet.Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace CustodialWallets.UnitTests.Users;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IValidator<RegisterUserCommand>> _validatorMock = new();
    private readonly Mock<ILogger<RegisterUserCommand>> _loggerMock = new();
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        
        _handler = new RegisterUserCommandHandler(
            _userRepositoryMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowDuplicateUserException_WhenEmailExists()
    {
        // Arrange
        const string email = "existing@example.com";
        var command = new RegisterUserCommand(email);

        _userRepositoryMock
            .Setup(r => r.GetByEmail(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Email = email });

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateUserException>(
            () => _handler.Handle(command, CancellationToken.None));

        _userRepositoryMock.Verify(
            r => r.Add(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRegisterNewUser_WhenEmailIsUnique()
    {
        // Arrange
        const string email = "new@example.com";
        var command = new RegisterUserCommand(email);
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(r => r.GetByEmail(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(r => r.Add(It.Is<User>(u => u.Email == email), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => user.Id = userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(userId, result.UserId);
        Assert.Equal(email, result.Email);
        Assert.Equal(0, result.Balance);

        _userRepositoryMock.Verify(
            r => r.Add(It.Is<User>(u => u.Email == email), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogInformation_WhenUserRegistered()
    {
        // Arrange
        var email = "new@example.com";
        var command = new RegisterUserCommand(email);

        _userRepositoryMock
            .Setup(r => r.GetByEmail(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains($"Зарегистрирован новый пользователь: {email}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}