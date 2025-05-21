using System;
using System.Threading;
using System.Threading.Tasks;
using CustodialWallet.Domain.Entities;

namespace CustodialWallet.Domain.Interfaces;

public interface IUserRepository
{
    Task<bool> Exists(Guid userId, CancellationToken cancellationToken);
    
    Task<User> GetByEmail(string email, CancellationToken cancellationToken);  
    
    Task Add(User user, CancellationToken cancellationToken);
}