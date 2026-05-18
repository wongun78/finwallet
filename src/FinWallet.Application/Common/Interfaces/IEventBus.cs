namespace FinWallet.Application.Common.Interfaces;

public interface IEventBus
{
    Task PublishAsync<T>(T message, CancellationToken ct) where T : class;
}
