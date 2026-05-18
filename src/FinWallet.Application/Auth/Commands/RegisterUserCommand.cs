using FinWallet.Application.Common.Exceptions;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Interfaces.Repositories;
using FinWallet.Application.Common.Models;
using FinWallet.Application.Common.Options;
using FinWallet.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace FinWallet.Application.Auth.Commands;

public sealed record RegisterUserCommand(string Email, string Password, string? IpAddress) : IRequest<AuthResultDto>;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);
    }
}

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResultDto>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly RefreshTokenOptions _refreshTokenOptions;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokens,
        IOptions<RefreshTokenOptions> refreshTokenOptions,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _refreshTokens = refreshTokens;
        _refreshTokenOptions = refreshTokenOptions.Value;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResultDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _users.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("Email da ton tai.");
        }

        var user = new User(request.Email, _passwordHasher.Hash(request.Password));
        _users.Add(user);

        var refreshToken = _refreshTokenService.GenerateToken();
        var refreshHash = _refreshTokenService.HashToken(refreshToken);
        var refreshExpires = _dateTimeProvider.UtcNow.AddDays(_refreshTokenOptions.Days);
        var tokenEntity = new RefreshToken(user.Id, refreshHash, refreshExpires, request.IpAddress);
        _refreshTokens.Add(tokenEntity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenService.GenerateToken(user.Id, user.Email);

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
