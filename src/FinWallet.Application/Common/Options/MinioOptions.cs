namespace FinWallet.Application.Common.Options;

public sealed class MinioOptions
{
    public const string SectionName = "Minio";

    public string Endpoint { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string Bucket { get; set; } = "finwallet-receipts";
    public bool UseSsl { get; set; }
    public string PublicEndpoint { get; set; } = "";
}
