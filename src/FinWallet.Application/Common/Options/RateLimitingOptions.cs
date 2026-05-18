namespace FinWallet.Application.Common.Options;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public bool Enabled { get; set; } = true;
    public int RequestsPerMinute { get; set; } = 120;
}
