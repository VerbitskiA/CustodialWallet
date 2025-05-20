namespace CustodialWallet.Application.Users.DTOs;

public record BalanceDto(  
    Guid UserId,  
    List<WalletBalanceDto> Wallets 
);  

public record WalletBalanceDto(  
    string Currency,  
    decimal Balance  
); 

/// <summary>
/// Упрощенная моделька для тестового
/// </summary>
/// <param name="UserId"></param>
/// <param name="Balance"></param>
public record UserBalanceResponse(Guid UserId, decimal Balance);