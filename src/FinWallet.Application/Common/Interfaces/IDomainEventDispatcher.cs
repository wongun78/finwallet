using FinWallet.Domain.Events;

namespace FinWallet.Application.Common.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken ct);
}
