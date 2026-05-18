using FinWallet.Domain.Enums;
using FinWallet.Domain.Events;
using FinWallet.Domain.Exceptions;
using FinWallet.Domain.Primitives;

namespace FinWallet.Domain.Entities;

public class Wallet : BaseEntity, IAggregateRoot
{
    private Wallet()
    {
    }

    public Wallet(Guid userId, string currency)
    {
        UserId = userId;
        Currency = currency.Trim().ToUpperInvariant();
        Status = WalletStatus.Active;
        Balance = 0m;
    }

    public Guid UserId { get; private set; }
    public string Currency { get; private set; } = "VND";
    public decimal Balance { get; private set; }
    public WalletStatus Status { get; private set; }
    public ICollection<WalletTransaction> Transactions { get; private set; } = new List<WalletTransaction>();

    public WalletTransaction ApplyCredit(decimal amount, string reference, TransactionType type, Guid? counterpartyWalletId = null)
    {
        if (amount <= 0)
        {
            throw new DomainException("So tien phai lon hon 0.");
        }

        return ApplyTransaction(amount, reference, type, counterpartyWalletId, isDebit: false);
    }

    public WalletTransaction ApplyDebit(decimal amount, string reference, TransactionType type, Guid? counterpartyWalletId = null)
    {
        if (amount <= 0)
        {
            throw new DomainException("So tien phai lon hon 0.");
        }

        if (Balance < amount)
        {
            throw new DomainException("So du khong du.");
        }

        return ApplyTransaction(amount, reference, type, counterpartyWalletId, isDebit: true);
    }

    private WalletTransaction ApplyTransaction(
        decimal amount,
        string reference,
        TransactionType type,
        Guid? counterpartyWalletId,
        bool isDebit)
    {
        if (Status != WalletStatus.Active)
        {
            throw new DomainException("Vi khong hoat dong.");
        }

        var balanceBefore = Balance;
        var balanceAfter = isDebit ? balanceBefore - amount : balanceBefore + amount;
        Balance = balanceAfter;

        var tx = new WalletTransaction(
            Id,
            type,
            amount,
            balanceBefore,
            balanceAfter,
            reference,
            counterpartyWalletId);

        Transactions.Add(tx);

        AddDomainEvent(new WalletTransactionCreatedEvent(tx.Id, Id, UserId, Currency, type, amount, balanceAfter));
        Touch();

        return tx;
    }

    public void Suspend()
    {
        Status = WalletStatus.Suspended;
        Touch();
    }

    public void Close()
    {
        Status = WalletStatus.Closed;
        Touch();
    }
}
