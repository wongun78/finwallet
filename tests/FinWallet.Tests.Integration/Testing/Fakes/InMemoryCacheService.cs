using System.Collections.Concurrent;
using FinWallet.Application.Common.Interfaces;

namespace FinWallet.Tests.Integration.Testing.Fakes;

public sealed class InMemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, object> _cache = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken ct)
    {
        if (_cache.TryGetValue(key, out var value) && value is T typed)
        {
            return Task.FromResult<T?>(typed);
        }

        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct)
    {
        _cache[key] = value!;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct)
    {
        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
