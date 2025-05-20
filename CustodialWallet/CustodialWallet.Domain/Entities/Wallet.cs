using CustodialWallet.Domain.Exceptions;

namespace CustodialWallet.Domain.Entities;

public class Wallet  
{  
    public int Id { get; set; }  
    public Guid UserId { get; set; }  
    
    public User User { get; set; }  
    public string Currency { get; set; }  // тикер валюты e.g. BTC, ETH, USDT 
    public decimal Balance { get; private set; } = 0; // возможно стоит делать String или кастомный тип
    
    public void Deposit(decimal amount)  
    {  
        if (amount <= 0)  
            throw new ArgumentException("Сумма должна быть положительной.");  

        Balance += amount;  
    }  
  
    public void Withdraw(decimal amount)  
    {  
        if (amount <= 0)  
            throw new ArgumentException("Сумма должна быть положительной.");  

        if (Balance < amount)  
            throw new InsufficientFundsException($"Недостаточно средств. Текущий баланс: {Balance} {Currency}");  

        Balance -= amount;  
    }  
} 