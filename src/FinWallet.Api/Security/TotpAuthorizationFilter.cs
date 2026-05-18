using FinWallet.Application.Common.Exceptions;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FinWallet.Api.Security;

public sealed class TotpAuthorizationFilter : IAsyncActionFilter
{
    private const string TotpHeader = "X-Totp-Code";

    private readonly ICurrentUserService _currentUser;
    private readonly IUserRepository _users;
    private readonly ITotpService _totpService;

    public TotpAuthorizationFilter(
        ICurrentUserService currentUser,
        IUserRepository users,
        ITotpService totpService)
    {
        _currentUser = currentUser;
        _users = users;
        _totpService = totpService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userId = _currentUser.GetUserId();
        if (userId is null)
        {
            throw new UnauthorizedException("Khong tim thay thong tin nguoi dung.");
        }

        var user = await _users.GetByIdAsync(userId.Value, context.HttpContext.RequestAborted);
        if (user is null)
        {
            throw new UnauthorizedException("Nguoi dung khong hop le.");
        }

        if (!user.IsTotpEnabled)
        {
            throw new ForbiddenException("TOTP chua duoc kich hoat.");
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(TotpHeader, out var code)
            || string.IsNullOrWhiteSpace(code))
        {
            throw new UnauthorizedException("Thieu ma TOTP.");
        }

        if (string.IsNullOrWhiteSpace(user.TotpSecret)
            || !_totpService.Validate(user.TotpSecret, code!))
        {
            throw new UnauthorizedException("Ma TOTP khong hop le.");
        }

        await next();
    }
}
