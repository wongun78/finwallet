using FinWallet.Application.Common.Interfaces;
using StackExchange.Redis;

namespace FinWallet.Infrastructure.Services;

public sealed class RedisDistributedLock : IDistributedLock
{
    private readonly IConnectionMultiplexer _redis;

    public RedisDistributedLock(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<IDisposable?> AcquireAsync(string key, TimeSpan ttl, CancellationToken ct)
    {
        var db = _redis.GetDatabase();
        var token = Guid.NewGuid().ToString();

        // Ghi chu: lock co TTL de tranh deadlock khi service bi crash.
        var acquired = await db.LockTakeAsync(key, token, ttl);
        if (!acquired)
        {
            return null;
        }

        return new RedisLockHandle(db, key, token);
    }

    private sealed class RedisLockHandle : IDisposable
    {
        private readonly IDatabase _db;
        private readonly string _key;
        private readonly string _token;
        private bool _disposed;

        public RedisLockHandle(IDatabase db, string key, string token)
        {
            _db = db;
            _key = key;
            _token = token;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _db.LockRelease(_key, _token);
            _disposed = true;
        }
    }
}
