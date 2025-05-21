namespace CustodialWallet.Application.Wallets.DTOs;

public record DepositFundsResponse(Guid UserId, decimal NewBalance);