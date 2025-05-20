using CustodialWallet.Domain.Entities;
using CustodialWallet.Domain.Interfaces;
using CustodialWallet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CustodialWallet.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository  
{
    public Task<bool> Exists(Guid userId, CancellationToken cancellationToken)
    {
        return context.Users.AsNoTracking().AnyAsync(x => x.Id == userId, cancellationToken);
    }

    public Task<User> GetByEmail(string email, CancellationToken cancellationToken)  
    {  
        return context.Users
            .AsNoTracking()  
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);  
    }  

    public async Task Add(User user, CancellationToken cancellationToken)  
    {  
        await context.Users.AddAsync(user, cancellationToken);  
        await context.SaveChangesAsync(cancellationToken);  
    }  
}