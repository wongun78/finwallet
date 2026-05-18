namespace FinWallet.Application.Common.Models;

public sealed record ReceiptPdfModel(
    Guid TransactionId,
    Guid WalletId,
    Guid UserId,
    string Currency,
    decimal Amount,
    decimal BalanceAfter,
    string Reference,
    DateTime CreatedAtUtc);
