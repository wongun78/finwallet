using FinWallet.Application.Common.Interfaces.Repositories;
using FinWallet.Domain.Entities;
using FinWallet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinWallet.Infrastructure.Repositories;

public sealed class WalletRepository : IWalletRepository
{
    private readonly ApplicationDbContext _dbContext;

    public WalletRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Wallet?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return _dbContext.Wallets.FirstOrDefaultAsync(w => w.Id == id, ct);
    }

    public Task<Wallet?> GetByIdWithTransactionsAsync(Guid id, CancellationToken ct)
    {
        return _dbContext.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.Id == id, ct);
    }

    public async Task<IReadOnlyList<Wallet>> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return await _dbContext.Wallets
            .Where(w => w.UserId == userId)
            .ToListAsync(ct);
    }

    public void Add(Wallet wallet)
    {
        _dbContext.Wallets.Add(wallet);
    }

    public void Update(Wallet wallet)
    {
        _dbContext.Wallets.Update(wallet);
    }
}
