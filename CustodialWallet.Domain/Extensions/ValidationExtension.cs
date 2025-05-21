using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;

namespace CustodialWallet.Domain.Extensions;

public static class ValidationExtension
{
    public static async Task ValidateAndThrowUserFriendlyErrorAsync<T>(this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(instance, cancellationToken);
        ThrowValidationExceptionIfNotValid(validationResult);
    }

    public static async Task ValidateAndThrowUserFriendlyErrorAsync<T>(this IValidator<T> validator,
        T instance,
        Action<ValidationStrategy<T>> options,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(instance, options, cancellationToken);
        ThrowValidationExceptionIfNotValid(validationResult);
    }

    public static async Task ValidateAndThrowUserFriendlyErrorAsync<T>(this IValidator<T> validator,
        T instance,
        Action<ValidationContext<T>> context,
        CancellationToken cancellationToken = default)
    {
        var contextInner = new ValidationContext<T>(instance);
        context(contextInner);

        var validationResult = await validator.ValidateAsync(contextInner, cancellationToken);

        ThrowValidationExceptionIfNotValid(validationResult);
    }

    private static void ThrowValidationExceptionIfNotValid(ValidationResult validationResult)
    {
        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                string.Join(
                    Environment.NewLine,
                    validationResult.Errors.Select(x => x.ErrorMessage)
                ),
                validationResult.Errors
            );
        }
    }
}