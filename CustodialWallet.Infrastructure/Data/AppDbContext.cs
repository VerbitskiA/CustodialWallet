using CustodialWallet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustodialWallet.Infrastructure.Data;

public class AppDbContext : DbContext  
{  
    public DbSet<User> Users { get; set; }  
    public DbSet<Wallet> Wallets { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }  
}  