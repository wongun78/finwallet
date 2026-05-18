namespace FinWallet.Application.Common.Interfaces;

public interface ITotpService
{
    string GenerateSecret();
    string BuildOtpAuthUri(string issuer, string email, string secret);
    bool Validate(string secret, string code);
}
