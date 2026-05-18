using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Transactions.Events;
using FinWallet.Domain.Events;

namespace FinWallet.Infrastructure.Services;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IEventBus _eventBus;

    public DomainEventDispatcher(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken ct)
    {
        foreach (var @event in events)
        {
            switch (@event)
            {
                case WalletTransactionCreatedEvent walletTx:
                    await _eventBus.PublishAsync(new TransactionCreatedIntegrationEvent(
                        walletTx.TransactionId,
                        walletTx.WalletId,
                        walletTx.UserId,
                        walletTx.Currency,
                        walletTx.Amount,
                        walletTx.Type.ToString(),
                        walletTx.OccurredAtUtc),
                        ct);
                    break;
            }
        }
    }
}
