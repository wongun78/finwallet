using FinWallet.Application.Common.Interfaces.Repositories;
using FinWallet.Domain.Entities;
using FinWallet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinWallet.Infrastructure.Repositories;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TransactionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<WalletTransaction?> GetByIdAsync(Guid transactionId, CancellationToken ct)
    {
        return _dbContext.WalletTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == transactionId, ct);
    }

    public async Task<IReadOnlyList<WalletTransaction>> GetByWalletIdAsync(
        Guid walletId,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        return await _dbContext.WalletTransactions
            .AsNoTracking()
            .Where(t => t.WalletId == walletId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<WalletTransaction>> GetAllByWalletIdAsync(Guid walletId, CancellationToken ct)
    {
        return await _dbContext.WalletTransactions
            .AsNoTracking()
            .Where(t => t.WalletId == walletId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public void Add(WalletTransaction tx)
    {
        _dbContext.WalletTransactions.Add(tx);
    }
}
