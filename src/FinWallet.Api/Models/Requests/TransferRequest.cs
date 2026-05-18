namespace FinWallet.Api.Models.Requests;

public sealed record TransferRequest(Guid FromWalletId, Guid ToWalletId, decimal Amount, string Reference);
