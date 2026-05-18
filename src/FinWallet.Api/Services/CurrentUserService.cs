using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FinWallet.Application.Common.Interfaces;

namespace FinWallet.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var value = user?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user?.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(value, out var id) ? id : null;
    }

    public string? GetEmail()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirstValue(ClaimTypes.Email)
            ?? user?.FindFirstValue(JwtRegisteredClaimNames.Email);
    }
}
