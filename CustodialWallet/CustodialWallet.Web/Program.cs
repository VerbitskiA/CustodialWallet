using CustodialWallet.Application;
using CustodialWallet.Application.Settings;
using CustodialWallet.Infrastructure;
using CustodialWallet.Infrastructure.Data;
using CustodialWallet.Web.Middlewares;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<CurrencySettings>(
    builder.Configuration.GetSection(nameof(CurrencySettings)));

builder.Services
    .AddApplication()  
    .AddInfrastructure(builder.Configuration); 

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();