using FinWallet.Application.Common.Interfaces;
using FinWallet.Tests.Integration.Testing.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace FinWallet.Tests.Integration.Testing;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("finwallet_test")
        .WithUsername("finwallet")
        .WithPassword("finwallet")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = _postgres.GetConnectionString(),
                ["MassTransit:UseInMemoryBus"] = "true",
                ["RateLimiting:Enabled"] = "false",
                ["Seed:Enabled"] = "false",
                ["Jwt:Issuer"] = "FinWallet",
                ["Jwt:Audience"] = "FinWallet",
                ["Jwt:Key"] = "test-secret-key-very-long",
                ["Jwt:ExpiryMinutes"] = "60"
            };

            configBuilder.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IRateLimitService, NoopRateLimitService>();
            services.AddSingleton<ICacheService, InMemoryCacheService>();
            services.AddSingleton<IDistributedLock, InMemoryDistributedLock>();
            services.AddSingleton<IObjectStorage, InMemoryObjectStorage>();
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
