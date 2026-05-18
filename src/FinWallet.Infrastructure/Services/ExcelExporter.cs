using ClosedXML.Excel;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Domain.Entities;

namespace FinWallet.Infrastructure.Services;

public sealed class ExcelExporter : IExcelExporter
{
    public Task<byte[]> ExportTransactionsAsync(IReadOnlyList<WalletTransaction> transactions, string currency, CancellationToken ct)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Transactions");

        sheet.Cell(1, 1).Value = "TransactionId";
        sheet.Cell(1, 2).Value = "WalletId";
        sheet.Cell(1, 3).Value = "Type";
        sheet.Cell(1, 4).Value = "Status";
        sheet.Cell(1, 5).Value = "Amount";
        sheet.Cell(1, 6).Value = "BalanceBefore";
        sheet.Cell(1, 7).Value = "BalanceAfter";
        sheet.Cell(1, 8).Value = "Reference";
        sheet.Cell(1, 9).Value = "CreatedAtUtc";

        var row = 2;
        foreach (var tx in transactions)
        {
            sheet.Cell(row, 1).Value = tx.Id.ToString();
            sheet.Cell(row, 2).Value = tx.WalletId.ToString();
            sheet.Cell(row, 3).Value = tx.Type.ToString();
            sheet.Cell(row, 4).Value = tx.Status.ToString();
            sheet.Cell(row, 5).Value = tx.Amount;
            sheet.Cell(row, 6).Value = tx.BalanceBefore;
            sheet.Cell(row, 7).Value = tx.BalanceAfter;
            sheet.Cell(row, 8).Value = tx.Reference;
            sheet.Cell(row, 9).Value = tx.CreatedAtUtc.ToString("O");
            row++;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }
}
