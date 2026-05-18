using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace FinWallet.Infrastructure.HealthChecks;

public sealed class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;

    public RedisHealthCheck(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var endpoints = _redis.GetEndPoints().Select(ep => ep.ToString()).ToArray();
        var data = new Dictionary<string, object?>
        {
            ["endpoints"] = endpoints
        };

        try
        {
            var db = _redis.GetDatabase();
            var latency = await db.PingAsync();
            data["latencyMs"] = latency.TotalMilliseconds;
            return HealthCheckResult.Healthy("Redis ok", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis loi", ex, data);
        }
    }
}
