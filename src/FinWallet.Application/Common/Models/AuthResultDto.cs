namespace FinWallet.Application.Common.Models;

public sealed record AuthResultDto(
    Guid UserId,
    string Email,
    string? AccessToken,
    DateTime? ExpiresAtUtc,
    bool RequiresTotp,
    string? RefreshToken,
    DateTime? RefreshExpiresAtUtc);
