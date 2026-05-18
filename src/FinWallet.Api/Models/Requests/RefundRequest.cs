namespace FinWallet.Api.Models.Requests;

public sealed record RefundRequest(decimal Amount, string Reference);
