namespace FinWallet.Application.Common.Interfaces;

public interface IRateLimitService
{
    Task<bool> IsRequestAllowedAsync(string key, int limit, TimeSpan window, CancellationToken ct);
}
