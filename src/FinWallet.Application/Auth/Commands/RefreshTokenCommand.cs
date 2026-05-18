using FinWallet.Application.Common.Exceptions;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Interfaces.Repositories;
using FinWallet.Application.Common.Models;
using FinWallet.Application.Common.Options;
using FinWallet.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;

namespace FinWallet.Application.Auth.Commands;

public sealed record RefreshTokenCommand(string RefreshToken, string? IpAddress) : IRequest<AuthResultDto>;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly RefreshTokenOptions _options;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokens,
        IJwtTokenService jwtTokenService,
        IDateTimeProvider dateTimeProvider,
        IOptions<RefreshTokenOptions> options,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenService = refreshTokenService;
        _refreshTokens = refreshTokens;
        _jwtTokenService = jwtTokenService;
        _dateTimeProvider = dateTimeProvider;
        _options = options.Value;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _refreshTokenService.HashToken(request.RefreshToken);
        var stored = await _refreshTokens.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (stored is null || !stored.IsActive)
        {
            throw new UnauthorizedException("Refresh token khong hop le.");
        }

        var user = stored.User;
        if (user is null)
        {
            throw new UnauthorizedException("Nguoi dung khong hop le.");
        }

        var newRefreshToken = _refreshTokenService.GenerateToken();
        var newHash = _refreshTokenService.HashToken(newRefreshToken);
        var newExpires = _dateTimeProvider.UtcNow.AddDays(_options.Days);

        stored.Revoke(request.IpAddress, newHash);
        _refreshTokens.Update(stored);

        _refreshTokens.Add(new RefreshToken(user.Id, newHash, newExpires, request.IpAddress));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenService.GenerateToken(user.Id, user.Email);

        return new AuthResultDto(
            user.Id,
            user.Email,
            accessToken.AccessToken,
            accessToken.ExpiresAtUtc,
            false,
            newRefreshToken,
            newExpires);
    }
}
