using CustodialWallet.Domain.Entities;
using CustodialWallet.Domain.Interfaces;
using CustodialWallet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CustodialWallet.Infrastructure.Repositories;

public class WalletRepository(AppDbContext context) : IWalletRepository
{
    public Task<Wallet?> GetById(int id, CancellationToken cancellationToken)
    {
        return context.Wallets.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public Task<Wallet?> GetByUserAndCurrency(Guid userId, string currency, CancellationToken cancellationToken)
    {
        return context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && w.Currency == currency,
            cancellationToken);
    }

    public Task<List<Wallet>> GetUserWallets(Guid userId, CancellationToken cancellationToken)
    {
        return context.Wallets
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task Add(Wallet wallet, CancellationToken cancellationToken)
    {
        await context.Wallets.AddAsync(wallet, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task Update(Wallet wallet, CancellationToken cancellationToken)
    {
        context.Wallets.Update(wallet);
        await context.SaveChangesAsync(cancellationToken);
    }
}