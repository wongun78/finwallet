using FinWallet.Application.Common.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Minio;

namespace FinWallet.Infrastructure.HealthChecks;

public sealed class MinioHealthCheck : IHealthCheck
{
    private readonly IMinioClient _minioClient;
    private readonly MinioOptions _options;

    public MinioHealthCheck(IMinioClient minioClient, IOptions<MinioOptions> options)
    {
        _minioClient = minioClient;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object?>
        {
            ["endpoint"] = _options.Endpoint,
            ["bucket"] = _options.Bucket,
            ["useSsl"] = _options.UseSsl
        };

        try
        {
            await _minioClient.ListBucketsAsync(cancellationToken);
            return HealthCheckResult.Healthy("MinIO ok", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("MinIO loi", ex, data);
        }
    }
}
