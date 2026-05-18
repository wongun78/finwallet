using FinWallet.Application.Common.Interfaces.Repositories;
using FinWallet.Domain.Entities;
using FinWallet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinWallet.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _dbContext;

    public RefreshTokenRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct)
    {
        return _dbContext.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);
    }

    public async Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return await _dbContext.RefreshTokens
            .Where(t => t.UserId == userId)
            .ToListAsync(ct);
    }

    public void Add(RefreshToken token)
    {
        _dbContext.RefreshTokens.Add(token);
    }

    public void Update(RefreshToken token)
    {
        _dbContext.RefreshTokens.Update(token);
    }
}
