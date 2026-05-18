using FinWallet.Application.Common.Interfaces;
using MassTransit;

namespace FinWallet.Infrastructure.Services;

public sealed class MassTransitEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventBus(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync<T>(T message, CancellationToken ct) where T : class
    {
        return _publishEndpoint.Publish(message, ct);
    }
}
