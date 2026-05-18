using FinWallet.Domain.Entities;

namespace FinWallet.Application.Common.Interfaces.Repositories;

public interface ITransactionRepository
{
    Task<WalletTransaction?> GetByIdAsync(Guid transactionId, CancellationToken ct);
    Task<IReadOnlyList<WalletTransaction>> GetByWalletIdAsync(
        Guid walletId,
        int page,
        int pageSize,
        CancellationToken ct);
    Task<IReadOnlyList<WalletTransaction>> GetAllByWalletIdAsync(Guid walletId, CancellationToken ct);
    void Add(WalletTransaction tx);
}
