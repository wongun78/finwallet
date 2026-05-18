using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Models;
using FinWallet.Application.Common.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinWallet.Infrastructure.Security;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly IDateTimeProvider _dateTimeProvider;

    public JwtTokenService(IOptions<JwtOptions> options, IDateTimeProvider dateTimeProvider)
    {
        _options = options.Value;
        _dateTimeProvider = dateTimeProvider;
    }

    public JwtTokenResult GenerateToken(Guid userId, string email)
    {
        var now = _dateTimeProvider.UtcNow;
        var expires = now.AddMinutes(_options.ExpiryMinutes);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds);

        var handler = new JwtSecurityTokenHandler();
        var accessToken = handler.WriteToken(token);

        return new JwtTokenResult(accessToken, expires);
    }
}
