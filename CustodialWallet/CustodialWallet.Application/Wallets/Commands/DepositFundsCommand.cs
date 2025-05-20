using CustodialWallet.Application.Wallets.DTOs;
using CustodialWallet.Domain.Entities;
using CustodialWallet.Domain.Extensions;
using CustodialWallet.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustodialWallet.Application.Wallets.Commands;

public record DepositFundsCommand(Guid UserId, string Currency, decimal Amount) : IRequest<DepositFundsResponse>;

public class DepositFundsCommandHandler(IWalletRepository walletRepository, 
    IValidator<DepositFundsCommand> validator,
    ILogger<DepositFundsCommand> logger) : IRequestHandler<DepositFundsCommand, DepositFundsResponse>
{
    public async Task<DepositFundsResponse> Handle(DepositFundsCommand request, CancellationToken ct)
    {
        await validator.ValidateAndThrowUserFriendlyErrorAsync(request, ct);
        
        var wallet = await walletRepository.GetByUserAndCurrency(request.UserId, request.Currency, ct);

        if (wallet is null)
        {
            wallet = new Wallet
            {
                UserId = request.UserId,
                Currency = request.Currency
            };

            await walletRepository.Add(wallet, ct);
        }

        wallet.Deposit(request.Amount);
        await walletRepository.Update(wallet, ct);

        logger.LogInformation("Совершен новый депозит от пользователя с Id {UserId} на сумму {Amount} {Currency}", 
            request.UserId,
            request.Amount,
            request.Currency);
        
        return new DepositFundsResponse(wallet.UserId, wallet.Balance);
    }
}