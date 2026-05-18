using System.Text.Json;
using FinWallet.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace FinWallet.Infrastructure.Services;

public sealed class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct)
    {
        var data = await _cache.GetAsync(key, ct);
        if (data is null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(data);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };

        await _cache.SetAsync(key, bytes, options, ct);
    }

    public Task RemoveAsync(string key, CancellationToken ct)
    {
        return _cache.RemoveAsync(key, ct);
    }
}
