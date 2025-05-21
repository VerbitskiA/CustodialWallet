using System.Net;
using CustodialWallet.Domain.Exceptions;
using FluentValidation;

namespace CustodialWallet.Web.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (DuplicateUserException ex)
        {
            await HandleExceptionAsync(httpContext,
                ex.Message,
                HttpStatusCode.Conflict,
                ex.Message);
        }
        catch (InsufficientFundsException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new
            {
                Error = "Insufficient funds"
            });
        }
        catch (ValidationException ex)
        {
            await HandleExceptionAsync(httpContext,
                ex.Message,
                HttpStatusCode.BadRequest,
                ex.Message);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext,
                ex.Message,
                HttpStatusCode.InternalServerError,
                "Internal server error");
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, string exceptionMessage, HttpStatusCode httpStatusCode, string message)
    {
        logger.LogError(exceptionMessage);

        var response = context.Response;

        response.ContentType = "application/json";
        response.StatusCode = (int)httpStatusCode;

        var errorDto = new ErrorDto((int)httpStatusCode, exceptionMessage);

        await response.WriteAsJsonAsync(errorDto);
    }
}

public record ErrorDto(int StatusCode, string Message);