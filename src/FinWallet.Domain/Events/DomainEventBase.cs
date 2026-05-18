namespace FinWallet.Domain.Events;

public abstract class DomainEventBase : IDomainEvent
{
    protected DomainEventBase()
    {
        OccurredAtUtc = DateTime.UtcNow;
    }

    public DateTime OccurredAtUtc { get; }
}
