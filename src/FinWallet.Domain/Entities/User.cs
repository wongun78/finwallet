using FinWallet.Domain.Enums;
using FinWallet.Domain.Exceptions;
using FinWallet.Domain.Primitives;

namespace FinWallet.Domain.Entities;

public class User : BaseEntity, IAggregateRoot
{
    private User()
    {
    }

    public User(string email, string passwordHash)
    {
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Status = UserStatus.Active;
    }

    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsTotpEnabled { get; private set; }
    public string? TotpSecret { get; private set; }
    public UserStatus Status { get; private set; }
    public ICollection<Wallet> Wallets { get; private set; } = new List<Wallet>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    public void UpdatePasswordHash(string passwordHash)
    {
        // Ghi chu: hash duoc tao tu PasswordHasher.
        PasswordHash = passwordHash;
        Touch();
    }

    public void SetupTotpSecret(string secret)
    {
        TotpSecret = secret;
        Touch();
    }

    public void EnableTotp()
    {
        if (string.IsNullOrWhiteSpace(TotpSecret))
        {
            throw new DomainException("TotpSecret chua duoc thiet lap.");
        }

        IsTotpEnabled = true;
        Touch();
    }

    public void DisableTotp()
    {
        IsTotpEnabled = false;
        Touch();
    }

    public void Lock()
    {
        Status = UserStatus.Locked;
        Touch();
    }

    public void AddRefreshToken(RefreshToken token)
    {
        RefreshTokens.Add(token);
        Touch();
    }
}
