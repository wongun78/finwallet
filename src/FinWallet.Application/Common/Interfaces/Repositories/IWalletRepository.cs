using FinWallet.Domain.Entities;

namespace FinWallet.Application.Common.Interfaces.Repositories;

public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Wallet?> GetByIdWithTransactionsAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Wallet>> GetByUserIdAsync(Guid userId, CancellationToken ct);
    void Add(Wallet wallet);
    void Update(Wallet wallet);
}
