using System.Reflection;
using CustodialWallet.Application.Settings;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace CustodialWallet.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    { 
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        return services;
    }
}