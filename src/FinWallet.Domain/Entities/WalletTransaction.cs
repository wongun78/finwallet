using FinWallet.Domain.Enums;
using FinWallet.Domain.Primitives;

namespace FinWallet.Domain.Entities;

public class WalletTransaction : BaseEntity
{
    private WalletTransaction()
    {
    }

    public WalletTransaction(
        Guid walletId,
        TransactionType type,
        decimal amount,
        decimal balanceBefore,
        decimal balanceAfter,
        string reference,
        Guid? counterpartyWalletId)
    {
        WalletId = walletId;
        Type = type;
        Amount = amount;
        BalanceBefore = balanceBefore;
        BalanceAfter = balanceAfter;
        Reference = reference;
        CounterpartyWalletId = counterpartyWalletId;
        Status = TransactionStatus.Completed;
    }

    public Guid WalletId { get; private set; }
    public Wallet Wallet { get; private set; } = null!;
    public TransactionType Type { get; private set; }
    public TransactionStatus Status { get; private set; }
    public decimal Amount { get; private set; }
    public decimal BalanceBefore { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public Guid? CounterpartyWalletId { get; private set; }
}
