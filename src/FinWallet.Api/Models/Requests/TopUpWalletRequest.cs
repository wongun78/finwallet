namespace FinWallet.Api.Models.Requests;

public sealed record TopUpWalletRequest(decimal Amount, string Reference);
