using FinWallet.Application.Common.Exceptions;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Interfaces.Repositories;
using FinWallet.Application.Common.Models;
using MediatR;

namespace FinWallet.Application.Wallets.Queries;

public sealed record ExportWalletTransactionsQuery(Guid WalletId) : IRequest<ExportFileResult>;

public sealed class ExportWalletTransactionsQueryHandler
    : IRequestHandler<ExportWalletTransactionsQuery, ExportFileResult>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IWalletRepository _wallets;
    private readonly ITransactionRepository _transactions;
    private readonly IExcelExporter _excelExporter;

    public ExportWalletTransactionsQueryHandler(
        ICurrentUserService currentUser,
        IWalletRepository wallets,
        ITransactionRepository transactions,
        IExcelExporter excelExporter)
    {
        _currentUser = currentUser;
        _wallets = wallets;
        _transactions = transactions;
        _excelExporter = excelExporter;
    }

    public async Task<ExportFileResult> Handle(ExportWalletTransactionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        if (userId is null)
        {
            throw new UnauthorizedException("Khong tim thay thong tin nguoi dung.");
        }

        var wallet = await _wallets.GetByIdAsync(request.WalletId, cancellationToken);
        if (wallet is null)
        {
            throw new NotFoundException("Khong tim thay vi.");
        }

        if (wallet.UserId != userId)
        {
            throw new ForbiddenException("Khong du quyen truy cap vi.");
        }

        var transactions = await _transactions.GetAllByWalletIdAsync(wallet.Id, cancellationToken);
        var content = await _excelExporter.ExportTransactionsAsync(transactions, wallet.Currency, cancellationToken);

        var fileName = $"transactions-{wallet.Id}.xlsx";
        return new ExportFileResult(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}
