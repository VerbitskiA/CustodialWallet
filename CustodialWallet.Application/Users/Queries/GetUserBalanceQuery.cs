using CustodialWallet.Application.Users.DTOs;
using CustodialWallet.Domain.Entities;
using CustodialWallet.Domain.Extensions;
using CustodialWallet.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace CustodialWallet.Application.Users.Queries;

public record GetUserBalanceQuery(Guid UserId, string? Currency = null) : IRequest<BalanceDto>;

public class GetUserBalanceQueryHandler(IWalletRepository walletRepository, IValidator<GetUserBalanceQuery> validator)
    : IRequestHandler<GetUserBalanceQuery, BalanceDto>
{
    public async Task<BalanceDto> Handle(GetUserBalanceQuery request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowUserFriendlyErrorAsync(request, cancellationToken);

        List<Wallet> wallets;
        if (string.IsNullOrEmpty(request.Currency))
        {
            wallets = await walletRepository.GetUserWallets(request.UserId, cancellationToken);
        }
        else
        {
            var wallet = await walletRepository.GetByUserAndCurrency(
                request.UserId,
                request.Currency, cancellationToken);

            wallets = wallet != null ? new List<Wallet> { wallet } : new List<Wallet>();
        }

        var walletDtos = wallets
            .Select(w => new WalletBalanceDto(w.Currency, w.Balance))
            .ToList();

        return new BalanceDto(request.UserId, walletDtos);
    }
}