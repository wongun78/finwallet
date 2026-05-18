using FinWallet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FinWallet.Infrastructure.HealthChecks;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _dbContext;

    public DatabaseHealthCheck(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var data = new Dictionary<string, object?>
        {
            ["provider"] = _dbContext.Database.ProviderName,
            ["dataSource"] = connection.DataSource,
            ["database"] = connection.Database
        };

        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Database ok", data)
                : HealthCheckResult.Unhealthy("Database khong ket noi duoc", data: data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database loi", ex, data);
        }
    }
}
