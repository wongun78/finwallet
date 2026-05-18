namespace FinWallet.Application.Common.Options;

public sealed class WebhookOptions
{
    public const string SectionName = "Webhook";

    public bool Enabled { get; set; }
    public string Endpoint { get; set; } = "";
    public string Secret { get; set; } = "";
}
