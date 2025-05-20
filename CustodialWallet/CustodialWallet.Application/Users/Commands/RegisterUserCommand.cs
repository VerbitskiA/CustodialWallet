using CustodialWallet.Application.Users.DTOs;
using CustodialWallet.Domain.Entities;
using CustodialWallet.Domain.Exceptions;
using CustodialWallet.Domain.Extensions;
using CustodialWallet.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CustodialWallet.Application.Users.Commands;

public record RegisterUserCommand(string Email) : IRequest<RegisterUserResponse>;

public class RegisterUserCommandHandler(IUserRepository userRepository,
    IValidator<RegisterUserCommand> validator,
    ILogger<RegisterUserCommand> logger) : IRequestHandler<RegisterUserCommand, RegisterUserResponse>  
{
    public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)  
    {  
        await validator.ValidateAndThrowUserFriendlyErrorAsync(request, cancellationToken);
        
        var existingUser = await userRepository.GetByEmail(request.Email, cancellationToken);
        
        if (existingUser is not null)  
            throw new DuplicateUserException($"Email {request.Email} уже занят.");  
  
        var user = new User  
        {   
            Email = request.Email  
        };  

        await userRepository.Add(user, cancellationToken);

        logger.LogInformation("Зарегистрирован новый пользователь: {Email}", user.Email);
        
        // В случае если кошелек мультивалютный можно создать дефолтные кошельки(адреса) для депозитов
        // Сейчас просто ставим баланс 0 - при первом депозите создастся кошелек
        
        return new RegisterUserResponse(user.Id, user.Email, 0);
    }  
} 