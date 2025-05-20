namespace CustodialWallet.Application.Wallets.DTOs;

public record WithdrawFundsResponse(Guid UserId, decimal NewBalance);