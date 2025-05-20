namespace CustodialWallet.Application.Wallets.DTOs;

public record WithdrawResponseDto(  
    Guid UserId,  
    string Currency,  
    decimal Amount,  
    decimal NewBalance
);