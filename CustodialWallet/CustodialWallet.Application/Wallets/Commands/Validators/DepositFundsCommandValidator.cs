using CustodialWallet.Application.Settings;
using CustodialWallet.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace CustodialWallet.Application.Wallets.Commands.Validators;

public class DepositFundsCommandValidator : AbstractValidator<DepositFundsCommand>
{
    private readonly IUserRepository _userRepository;
    
    public DepositFundsCommandValidator(IUserRepository userRepository, IOptionsSnapshot<CurrencySettings> currencyOptions)
    {
        var allowedCurrencies = currencyOptions.Value.AllowedCurrencies;
        
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User Id обязателен.")
            .MustAsync(async (x, ct) => await UserExists(x, ct))
            .WithMessage(x => $"Пользователь с Id {x.UserId} не найден.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Валюта обязательна.")
            .Must(currency => allowedCurrencies.Contains(currency))
            .WithMessage("Недопустимая валюта.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Сумма должна быть больше 0.");
        
        _userRepository = userRepository;
    }
    
    private Task<bool> UserExists(Guid userId, CancellationToken cancellationToken)
    {
        return _userRepository.Exists(userId, cancellationToken);
    }
}