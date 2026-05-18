namespace FinWallet.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredAtUtc { get; }
}
