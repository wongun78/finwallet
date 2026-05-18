using QuestPDF.Helpers;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace FinWallet.Infrastructure.Storage;

public sealed class ReceiptPdfGenerator : IReceiptPdfGenerator
{
    public Task<byte[]> GenerateAsync(ReceiptPdfModel model, CancellationToken ct)
    {
        // Ghi chu: tao PDF bien lai don gian.
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(24);
                page.Size(PageSizes.A4);

                page.Content().Column(column =>
                {
                    column.Item().Text("FinWallet Receipt").FontSize(20).Bold();
                    column.Item().Text($"TransactionId: {model.TransactionId}");
                    column.Item().Text($"WalletId: {model.WalletId}");
                    column.Item().Text($"UserId: {model.UserId}");
                    column.Item().Text($"Amount: {model.Amount} {model.Currency}");
                    column.Item().Text($"BalanceAfter: {model.BalanceAfter} {model.Currency}");
                    column.Item().Text($"Reference: {model.Reference}");
                    column.Item().Text($"CreatedAt: {model.CreatedAtUtc:O}");
                });
            });
        });

        var pdf = document.GeneratePdf();
        return Task.FromResult(pdf);
    }
}
