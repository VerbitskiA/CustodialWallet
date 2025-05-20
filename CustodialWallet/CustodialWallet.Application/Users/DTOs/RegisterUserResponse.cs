namespace CustodialWallet.Application.Users.DTOs;

public record RegisterUserResponse(Guid UserId, string Email, decimal Balance);