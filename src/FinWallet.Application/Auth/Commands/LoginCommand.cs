using FinWallet.Application.Common.Exceptions;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Interfaces.Repositories;
using FinWallet.Application.Common.Models;
using FinWallet.Application.Common.Options;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace FinWallet.Application.Auth.Commands;

public sealed record LoginCommand(string Email, string Password, string? TotpCode, string? IpAddress)
    : IRequest<AuthResultDto>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITotpService _totpService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly RefreshTokenOptions _refreshTokenOptions;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        ITotpService totpService,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokens,
        IOptions<RefreshTokenOptions> refreshTokenOptions,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _totpService = totpService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _refreshTokens = refreshTokens;
        _refreshTokenOptions = refreshTokenOptions.Value;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            throw new UnauthorizedException("Thong tin dang nhap khong dung.");
        }

        var validPassword = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!validPassword)
        {
            throw new UnauthorizedException("Thong tin dang nhap khong dung.");
        }

        if (user.IsTotpEnabled)
        {
            if (string.IsNullOrWhiteSpace(request.TotpCode))
            {
                return new AuthResultDto(user.Id, user.Email, null, null, true, null, null);
            }

            if (string.IsNullOrWhiteSpace(user.TotpSecret) || !_totpService.Validate(user.TotpSecret, request.TotpCode))
            {
                throw new UnauthorizedException("Ma TOTP khong hop le.");
            }
        }

        var token = _jwtTokenService.GenerateToken(user.Id, user.Email);

        var refreshToken = _refreshTokenService.GenerateToken();
        var refreshHash = _refreshTokenService.HashToken(refreshToken);
        var refreshExpires = _dateTimeProvider.UtcNow.AddDays(_refreshTokenOptions.Days);
        _refreshTokens.Add(new Domain.Entities.RefreshToken(user.Id, refreshHash, refreshExpires, request.IpAddress));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(
            user.Id,
            user.Email,
            token.AccessToken,
            token.ExpiresAtUtc,
            false,
            refreshToken,
            refreshExpires);
    }
}
