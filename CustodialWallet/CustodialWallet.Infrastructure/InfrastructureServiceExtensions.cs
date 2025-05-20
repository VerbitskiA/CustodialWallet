using CustodialWallet.Domain.Interfaces;
using CustodialWallet.Infrastructure.Data;
using CustodialWallet.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore; 
using Microsoft.Extensions.Configuration;

namespace CustodialWallet.Infrastructure;

public static class InfrastructureServiceExtensions  
{  
    public static IServiceCollection AddInfrastructure(  
        this IServiceCollection services,  
        IConfiguration configuration)  
    {  
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgresConnection")));

        services.AddScoped<IUserRepository, UserRepository>();  
        services.AddScoped<IWalletRepository, WalletRepository>();  

        // Дополнительные сервисы  
        //services.AddTransient<ICryptoService, FakeCryptoService>();  

        return services;  
    }  
} 