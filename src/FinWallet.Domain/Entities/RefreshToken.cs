using FinWallet.Domain.Primitives;

namespace FinWallet.Domain.Entities;

public class RefreshToken : BaseEntity
{
    private RefreshToken()
    {
    }

    public RefreshToken(
        Guid userId,
        string tokenHash,
        DateTime expiresAtUtc,
        string? createdByIp)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedByIp = createdByIp;
    }

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public string? CreatedByIp { get; private set; }

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow <= ExpiresAtUtc;

    public void Revoke(string? ipAddress, string? replacedByTokenHash = null)
    {
        RevokedAtUtc = DateTime.UtcNow;
        RevokedByIp = ipAddress;
        ReplacedByTokenHash = replacedByTokenHash;
        Touch();
    }
}
