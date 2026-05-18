namespace FinWallet.Application.Common.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public bool ApplyMigrations { get; set; }
}
