using FinWallet.Domain.Entities;

namespace FinWallet.Application.Common.Interfaces;

public interface IExcelExporter
{
    Task<byte[]> ExportTransactionsAsync(IReadOnlyList<WalletTransaction> transactions, string currency, CancellationToken ct);
}
