namespace FinWallet.Application.Common.Options;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public bool Enabled { get; set; } = true;
    public string AdminEmail { get; set; } = "admin@finwallet.local";
    public string AdminPassword { get; set; } = "P@ssw0rd!";
    public bool SeedWallet { get; set; } = true;
}
