using CustodialWallet.Domain.Interfaces;
using FluentValidation;

namespace CustodialWallet.Application.Users.Queries.Validators;

public class GetUserBalanceQueryValidator : AbstractValidator<GetUserBalanceQuery> 
{
    private readonly IUserRepository _userRepository;

    public GetUserBalanceQueryValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User Id обязателен.")
            .MustAsync(async (x, ct) => await UserExists(x, ct))
            .WithMessage(x => $"Пользователь с Id {x.UserId} не найден.");
        
        _userRepository = userRepository;
    }
    
    private Task<bool> UserExists(Guid userId, CancellationToken cancellationToken)
    {
        return _userRepository.Exists(userId, cancellationToken);
    }
}