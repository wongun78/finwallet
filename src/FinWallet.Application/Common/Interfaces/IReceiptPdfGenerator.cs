using FinWallet.Application.Common.Models;

namespace FinWallet.Application.Common.Interfaces;

public interface IReceiptPdfGenerator
{
    Task<byte[]> GenerateAsync(ReceiptPdfModel model, CancellationToken ct);
}
