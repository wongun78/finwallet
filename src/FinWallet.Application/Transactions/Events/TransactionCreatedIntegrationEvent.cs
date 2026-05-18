namespace FinWallet.Application.Transactions.Events;

public sealed record TransactionCreatedIntegrationEvent(
    Guid TransactionId,
    Guid WalletId,
    Guid UserId,
    string Currency,
    decimal Amount,
    string Type,
    DateTime CreatedAtUtc);
