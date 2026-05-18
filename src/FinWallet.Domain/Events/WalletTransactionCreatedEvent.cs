using FinWallet.Domain.Enums;

namespace FinWallet.Domain.Events;

public sealed class WalletTransactionCreatedEvent : DomainEventBase
{
    public WalletTransactionCreatedEvent(
        Guid transactionId,
        Guid walletId,
        Guid userId,
        string currency,
        TransactionType type,
        decimal amount,
        decimal balanceAfter)
    {
        TransactionId = transactionId;
        WalletId = walletId;
        UserId = userId;
        Currency = currency;
        Type = type;
        Amount = amount;
        BalanceAfter = balanceAfter;
    }

    public Guid TransactionId { get; }
    public Guid WalletId { get; }
    public Guid UserId { get; }
    public string Currency { get; }
    public TransactionType Type { get; }
    public decimal Amount { get; }
    public decimal BalanceAfter { get; }
}
