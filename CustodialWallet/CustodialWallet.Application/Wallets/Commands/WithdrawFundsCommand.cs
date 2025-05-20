using CustodialWallet.Application.Wallets.DTOs;
using CustodialWallet.Domain.Exceptions;
using CustodialWallet.Domain.Extensions;
using CustodialWallet.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustodialWallet.Application.Wallets.Commands;

public record WithdrawFundsCommand(
    Guid UserId,
    string Currency,
    decimal Amount,
    string DestinationAddress
) : IRequest<WithdrawResponseDto>;

public class WithdrawFundsCommandHandler(IWalletRepository walletRepository,
        IValidator<WithdrawFundsCommand> validator,
        ILogger<WithdrawFundsCommandHandler> logger)
    : IRequestHandler<WithdrawFundsCommand, WithdrawResponseDto>
{
    public async Task<WithdrawResponseDto> Handle(WithdrawFundsCommand request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowUserFriendlyErrorAsync(request, cancellationToken);
        
        var wallet = await walletRepository.GetByUserAndCurrency(request.UserId, request.Currency, cancellationToken);

        if (wallet is null)
            throw new ValidationException($"Кошелек с валютой {request.Currency} не найден.");

        if (wallet.Balance < request.Amount)
            throw new InsufficientFundsException(
                $"Недостаточно средств. Текущий баланс: {wallet.Balance} {request.Currency}");

        wallet.Withdraw(request.Amount);
        await walletRepository.Update(wallet, cancellationToken);

        logger.LogInformation(
            "Вывод от пользователя {UserId} на сумму {Amount} {Currency} на адрес {DestinationAddress}",
            request.UserId,
            request.Amount, 
            request.Currency, 
            request.DestinationAddress);

        return new WithdrawResponseDto(
            request.UserId,
            request.Currency,
            request.Amount,
            wallet.Balance
        );
    }
}