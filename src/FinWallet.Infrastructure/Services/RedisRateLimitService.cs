using FinWallet.Application.Common.Interfaces;
using StackExchange.Redis;

namespace FinWallet.Infrastructure.Services;

public sealed class RedisRateLimitService : IRateLimitService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisRateLimitService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<bool> IsRequestAllowedAsync(string key, int limit, TimeSpan window, CancellationToken ct)
    {
        var db = _redis.GetDatabase();

        // Ghi chu: INCR + EXPIRE tao counter atomic, tranh race condition.
        var count = await db.StringIncrementAsync(key);

        if (count == 1)
        {
            await db.KeyExpireAsync(key, window);
        }

        return count <= limit;
    }
}
