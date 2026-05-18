namespace FinWallet.Application.Common.Models;

public sealed record JwtTokenResult(string AccessToken, DateTime ExpiresAtUtc);
