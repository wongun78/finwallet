using FinWallet.Application.Common.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace FinWallet.Infrastructure.HealthChecks;

public sealed class SeqHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SeqOptions _options;

    public SeqHealthCheck(IHttpClientFactory httpClientFactory, IOptions<SeqOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object?>
        {
            ["serverUrl"] = _options.ServerUrl
        };

        if (string.IsNullOrWhiteSpace(_options.ServerUrl))
        {
            return HealthCheckResult.Degraded("Seq chua duoc cau hinh", null, new Dictionary<string, object>(data!));
        }

        var url = _options.ServerUrl.TrimEnd('/') + "/api/health";
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(3);

        try
        {
            using var response = await client.GetAsync(url, cancellationToken);
            data["httpStatus"] = (int)response.StatusCode;
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Seq ok", data)
                : HealthCheckResult.Unhealthy("Seq khong phan hoi", data: data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Seq loi", ex, data);
        }
    }
}
