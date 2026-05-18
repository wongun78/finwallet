namespace FinWallet.Application.Transactions.Events;

public sealed record TransferRequestedIntegrationEvent(
    Guid TransferId,
    Guid FromWalletId,
    Guid ToWalletId,
    decimal Amount,
    string Reference,
    DateTime CreatedAtUtc);
