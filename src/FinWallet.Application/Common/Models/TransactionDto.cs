namespace FinWallet.Application.Common.Models;

public sealed record TransactionDto(
    Guid TransactionId,
    Guid WalletId,
    string Type,
    string Status,
    decimal Amount,
    decimal BalanceBefore,
    decimal BalanceAfter,
    string Reference,
    Guid? CounterpartyWalletId,
    DateTime CreatedAtUtc);
