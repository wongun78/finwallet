using FinWallet.Application.Common.Exceptions;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Interfaces.Repositories;
using MediatR;

namespace FinWallet.Application.Auth.Commands;

public sealed record RevokeRefreshTokenCommand(string RefreshToken, string? IpAddress) : IRequest<bool>;

public sealed class RevokeRefreshTokenCommandHandler : IRequestHandler<RevokeRefreshTokenCommand, bool>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeRefreshTokenCommandHandler(
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenService = refreshTokenService;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _refreshTokenService.HashToken(request.RefreshToken);
        var stored = await _refreshTokens.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (stored is null || !stored.IsActive)
        {
            throw new UnauthorizedException("Refresh token khong hop le.");
        }

        stored.Revoke(request.IpAddress);
        _refreshTokens.Update(stored);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
