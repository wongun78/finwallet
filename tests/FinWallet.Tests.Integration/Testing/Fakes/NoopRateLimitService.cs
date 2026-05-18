using FinWallet.Application.Common.Interfaces;

namespace FinWallet.Tests.Integration.Testing.Fakes;

public sealed class NoopRateLimitService : IRateLimitService
{
    public Task<bool> IsRequestAllowedAsync(string key, int limit, TimeSpan window, CancellationToken ct)
    {
        return Task.FromResult(true);
    }
}
