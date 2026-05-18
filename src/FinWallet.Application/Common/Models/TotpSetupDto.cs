namespace FinWallet.Application.Common.Models;

public sealed record TotpSetupDto(string Secret, string OtpAuthUri);
