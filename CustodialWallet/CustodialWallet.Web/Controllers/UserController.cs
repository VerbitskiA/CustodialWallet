using CustodialWallet.Application.Users.Commands;
using CustodialWallet.Application.Users.DTOs;
using CustodialWallet.Application.Users.Queries;
using CustodialWallet.Application.Wallets.Commands;
using CustodialWallet.Application.Wallets.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CustodialWallet.Web.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IMediator mediator) : ControllerBase
{
    private const string DefaultTicker = "BTC";
    private const string DefaultWithdrawAddress = "default-withdraw-address";

    /// <summary>
    /// Метод регистрации пользователя.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<RegisterUserResponse>> Register(RegisterUserCommand request)
    {
        var result = await mediator.Send(request);

        return Ok(result);
    }

    [HttpGet("{userId}/balance")]
    public async Task<ActionResult<UserBalanceResponse>> GetBalance([FromRoute] Guid userId)
    {
        var result = await mediator.Send(new GetUserBalanceQuery(userId, DefaultTicker));

        // делаем простую модельку, без тикеров и мультивалютности
        return Ok(new UserBalanceResponse(result.UserId, result.Wallets.Count > 0 ? result.Wallets[0].Balance : 0));
    }

    [HttpPost("{userId}/deposit")]
    public async Task<ActionResult<DepositFundsResponse>> Deposit([FromRoute] Guid userId,
        [FromBody] DepositFundsRequest depositFundsRequest)
    {
        var result = await mediator.Send(new DepositFundsCommand(userId, DefaultTicker, depositFundsRequest.Amount));

        return Ok(result);
    }

    [HttpPost("{userId}/withdraw")]
    public async Task<ActionResult<WithdrawFundsResponse>> Withdraw([FromRoute] Guid userId,
        [FromBody] WithdrawFundsRequest withdrawFundsRequest)
    {
        var result = await mediator.Send(new WithdrawFundsCommand(userId, DefaultTicker, withdrawFundsRequest.Amount,
            DefaultWithdrawAddress));

        return Ok(new WithdrawFundsResponse(result.UserId, result.NewBalance));
    }
}