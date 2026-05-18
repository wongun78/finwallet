using FinWallet.Domain.Entities;

namespace FinWallet.Application.Common.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct);
    Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken ct);
    void Add(RefreshToken token);
    void Update(RefreshToken token);
}
