namespace FinWallet.Api.Models.Requests;

public sealed record PaymentRequest(decimal Amount, string Reference);
