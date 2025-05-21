using FluentValidation;

namespace CustodialWallet.Application.Users.Commands.Validators;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>  
{  
    public RegisterUserCommandValidator()  
    {  
        RuleFor(x => x.Email)  
            .NotEmpty()
            .WithMessage("Email обязателен.")  
            .EmailAddress()
            .WithMessage("Некорректный email.");
    }  
} 