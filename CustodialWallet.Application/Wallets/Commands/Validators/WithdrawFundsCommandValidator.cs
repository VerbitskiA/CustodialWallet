using CustodialWallet.Application.Settings;
using CustodialWallet.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace CustodialWallet.Application.Wallets.Commands.Validators;

public class WithdrawFundsCommandValidator : AbstractValidator<WithdrawFundsCommand>
{
    private readonly IUserRepository _userRepository;

    public WithdrawFundsCommandValidator(IOptionsSnapshot<CurrencySettings> currencyOptions, IUserRepository userRepository)
    {
        var allowedCurrencies = currencyOptions.Value.AllowedCurrencies;

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID обязателен.").MustAsync(async (x, ct) => await UserExists(x, ct))
            .WithMessage(x => $"Пользователь с Id {x.UserId} не найден.");
        ;

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Валюта обязательна.")
            .Must(currency => allowedCurrencies.Contains(currency))
            .WithMessage($"Недопустимая валюта. Допустимые: {string.Join(", ", allowedCurrencies)}.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Сумма должна быть больше 0.")
            .PrecisionScale(18, 8, false)
            .WithMessage("Сумма должна иметь до 8 знаков после запятой.");
            

        RuleFor(x => x.DestinationAddress)
            .NotEmpty()
            .WithMessage("Адрес кошелька обязателен.");

        _userRepository = userRepository;
    }

    private Task<bool> UserExists(Guid userId, CancellationToken cancellationToken)
    {
        return _userRepository.Exists(userId, cancellationToken);
    }
}