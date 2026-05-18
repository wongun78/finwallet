namespace FinWallet.Application.Common.Models;

public sealed record WalletDto
{
    public Guid WalletId { get; init; }
    public Guid UserId { get; init; }
    public string Currency { get; init; } = string.Empty;
    public decimal Balance { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
}
