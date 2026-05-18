namespace FinWallet.Application.Transactions.Events;

public sealed record TransferFailedIntegrationEvent(
    Guid TransferId,
    string Reason,
    DateTime FailedAtUtc);
