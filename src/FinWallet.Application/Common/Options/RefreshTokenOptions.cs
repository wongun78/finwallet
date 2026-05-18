namespace FinWallet.Application.Common.Options;

public sealed class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    public int Days { get; set; } = 7;
}
