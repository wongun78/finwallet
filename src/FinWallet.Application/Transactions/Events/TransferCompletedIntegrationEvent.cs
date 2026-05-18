namespace FinWallet.Application.Transactions.Events;

public sealed record TransferCompletedIntegrationEvent(
    Guid TransferId,
    DateTime CompletedAtUtc);
