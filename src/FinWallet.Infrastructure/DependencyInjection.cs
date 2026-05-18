using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Interfaces.Repositories;
using FinWallet.Application.Common.Options;
using FinWallet.Infrastructure.Persistence;
using FinWallet.Infrastructure.Repositories;
using FinWallet.Infrastructure.Security;
using FinWallet.Infrastructure.Services;
using FinWallet.Infrastructure.Storage;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;
using StackExchange.Redis;

namespace FinWallet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<MinioOptions>(configuration.GetSection(MinioOptions.SectionName));
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.Configure<RateLimitingOptions>(configuration.GetSection(RateLimitingOptions.SectionName));
        services.Configure<MassTransitOptions>(configuration.GetSection(MassTransitOptions.SectionName));
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<RefreshTokenOptions>(configuration.GetSection(RefreshTokenOptions.SectionName));
        services.Configure<SeqOptions>(configuration.GetSection(SeqOptions.SectionName));
        services.Configure<WebhookOptions>(configuration.GetSection(WebhookOptions.SectionName));

        var dbConnection = configuration.GetConnectionString("Database")
            ?? configuration["ConnectionStrings:Database"]
            ?? string.Empty;

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(dbConnection));

        var redisConnection = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>()?.ConnectionString
            ?? configuration.GetConnectionString("Redis")
            ?? "localhost:6379";

        var redisOptions = ConfigurationOptions.Parse(redisConnection, true);
        redisOptions.AbortOnConnectFail = false;
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions));
        services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);

        services.AddSingleton<IMinioClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MinioOptions>>().Value;
            return new MinioClient()
                .WithEndpoint(options.Endpoint)
                .WithCredentials(options.AccessKey, options.SecretKey)
                .WithSSL(options.UseSsl)
                .Build();
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<ITotpService, TotpService>();
        services.AddSingleton<IReceiptPdfGenerator, ReceiptPdfGenerator>();
        services.AddSingleton<IObjectStorage, MinioObjectStorage>();
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<IRateLimitService, RedisRateLimitService>();
        services.AddScoped<IDistributedLock, RedisDistributedLock>();
        services.AddScoped<IEventBus, MassTransitEventBus>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddSingleton<IWebhookSender, WebhookSender>();
        services.AddSingleton<IExcelExporter, ExcelExporter>();

        services.AddScoped<DataSeeder>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<TransactionCreatedConsumer>();
            x.AddSagaStateMachine<TransferStateMachine, TransferSagaState>()
                .InMemoryRepository();

            var mtOptions = configuration.GetSection(MassTransitOptions.SectionName)
                .Get<MassTransitOptions>() ?? new MassTransitOptions();

            if (mtOptions.UseInMemoryBus)
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });

                return;
            }

            var rabbit = configuration.GetSection(RabbitMqOptions.SectionName)
                .Get<RabbitMqOptions>() ?? new RabbitMqOptions();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbit.Host, rabbit.VirtualHost, h =>
                {
                    h.Username(rabbit.Username);
                    h.Password(rabbit.Password);
                });

                cfg.ReceiveEndpoint("finwallet-transactions", e =>
                {
                    e.ConfigureConsumer<TransactionCreatedConsumer>(context);
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                    e.UseDelayedRedelivery(r => r.Interval(2, TimeSpan.FromSeconds(10)));
                    e.UseInMemoryOutbox();
                });

                cfg.ReceiveEndpoint("finwallet-transfers", e =>
                {
                    e.StateMachineSaga(
                        context.GetRequiredService<TransferStateMachine>(),
                        context.GetRequiredService<ISagaRepository<TransferSagaState>>());
                });
            });
        });

        return services;
    }
}
