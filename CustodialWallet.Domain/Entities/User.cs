using System;

namespace CustodialWallet.Domain.Entities;

public class User
{
    public Guid Id { get; set; }  
    public string Email { get; set; }  
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
}