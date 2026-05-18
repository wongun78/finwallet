using FinWallet.Api.Models.Requests;
using FinWallet.Application.Auth.Commands;
using FinWallet.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinWallet.Api.Controllers;

/// <summary>
/// Nhom API xac thuc va TOTP.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Dang ky tai khoan moi va nhan JWT + refresh token.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// <code>
    /// {
    ///   "email": "admin@finwallet.local",
    ///   "password": "P@ssw0rd!123"
    /// }
    /// </code>
    /// Sample response:
    /// <code>
    /// {
    ///   "userId": "4d36a6b0-d1b7-4048-bdf5-86086c2f4348",
    ///   "email": "admin@finwallet.local",
    ///   "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    ///   "expiresAtUtc": "2026-05-12T09:30:00Z",
    ///   "requiresTotp": false,
    ///   "refreshToken": "f9b2a8c1d0e84f0d9f6b2f3d5e0a1234",
    ///   "refreshExpiresAtUtc": "2026-05-19T09:30:00Z"
    /// }
    /// </code>
    /// </remarks>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResultDto>> Register(RegisterRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _mediator.Send(new RegisterUserCommand(request.Email, request.Password, ip), ct);
        return Ok(result);
    }

    /// <summary>
    /// Dang nhap (ho tro TOTP neu da kich hoat).
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// <code>
    /// {
    ///   "email": "admin@finwallet.local",
    ///   "password": "P@ssw0rd!123",
    ///   "totpCode": "123456"
    /// }
    /// </code>
    /// Neu thieu TOTP, response se tra ve requiresTotp=true va khong co accessToken.
    /// </remarks>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login(LoginRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _mediator.Send(new LoginCommand(request.Email, request.Password, request.TotpCode, ip), ct);
        return Ok(result);
    }

    /// <summary>
    /// Tao secret de cau hinh TOTP.
    /// </summary>
    /// <remarks>
    /// Sample response:
    /// <code>
    /// {
    ///   "secret": "JBSWY3DPEHPK3PXP",
    ///   "otpAuthUri": "otpauth://totp/FinWallet:admin@finwallet.local?secret=JBSWY3DPEHPK3PXP&amp;issuer=FinWallet"
    /// }
    /// </code>
    /// </remarks>
    [Authorize]
    [HttpPost("totp/setup")]
    public async Task<ActionResult<TotpSetupDto>> SetupTotp(CancellationToken ct)
    {
        var result = await _mediator.Send(new SetupTotpCommand(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Xac nhan TOTP va kich hoat 2FA.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// <code>
    /// { "code": "123456" }
    /// </code>
    /// Sample response:
    /// <code>
    /// true
    /// </code>
    /// </remarks>
    [Authorize]
    [HttpPost("totp/confirm")]
    public async Task<ActionResult<bool>> ConfirmTotp(ConfirmTotpRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ConfirmTotpCommand(request.Code), ct);
        return Ok(result);
    }

    /// <summary>
    /// Lay access token moi tu refresh token.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResultDto>> Refresh(RefreshTokenRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken, ip), ct);
        return Ok(result);
    }

    /// <summary>
    /// Thu hoi refresh token (logout).
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<bool>> Logout(RevokeRefreshTokenRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _mediator.Send(new RevokeRefreshTokenCommand(request.RefreshToken, ip), ct);
        return Ok(result);
    }
}
