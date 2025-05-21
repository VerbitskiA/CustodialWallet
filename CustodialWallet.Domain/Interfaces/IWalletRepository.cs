using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustodialWallet.Domain.Entities;

namespace CustodialWallet.Domain.Interfaces;

public interface IWalletRepository  
{  
    Task<Wallet?> GetById(int id, CancellationToken cancellationToken);  
    Task<Wallet?> GetByUserAndCurrency(Guid userId, string currency, CancellationToken cancellationToken);  
    Task<List<Wallet>> GetUserWallets(Guid userId, CancellationToken cancellationToken);  
    Task Add(Wallet wallet, CancellationToken cancellationToken);  
    Task Update(Wallet wallet, CancellationToken cancellationToken);  
} 