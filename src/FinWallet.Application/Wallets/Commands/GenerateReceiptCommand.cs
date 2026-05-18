using FinWallet.Application.Common.Exceptions;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Interfaces.Repositories;
using FinWallet.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace FinWallet.Application.Wallets.Commands;

public sealed record GenerateReceiptCommand(Guid WalletId, Guid TransactionId) : IRequest<ReceiptDto>;

public sealed class GenerateReceiptCommandValidator : AbstractValidator<GenerateReceiptCommand>
{
    public GenerateReceiptCommandValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty();

        RuleFor(x => x.TransactionId)
            .NotEmpty();
    }
}

public sealed class GenerateReceiptCommandHandler : IRequestHandler<GenerateReceiptCommand, ReceiptDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IWalletRepository _wallets;
    private readonly ITransactionRepository _transactions;
    private readonly IReceiptPdfGenerator _receiptPdfGenerator;
    private readonly IObjectStorage _objectStorage;

    public GenerateReceiptCommandHandler(
        ICurrentUserService currentUser,
        IWalletRepository wallets,
        ITransactionRepository transactions,
        IReceiptPdfGenerator receiptPdfGenerator,
        IObjectStorage objectStorage)
    {
        _currentUser = currentUser;
        _wallets = wallets;
        _transactions = transactions;
        _receiptPdfGenerator = receiptPdfGenerator;
        _objectStorage = objectStorage;
    }

    public async Task<ReceiptDto> Handle(GenerateReceiptCommand request, CancellationToken cancellationToken)
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

        var transaction = await _transactions.GetByIdAsync(request.TransactionId, cancellationToken);
        if (transaction is null || transaction.WalletId != wallet.Id)
        {
            throw new NotFoundException("Khong tim thay giao dich.");
        }

        var pdfModel = new ReceiptPdfModel(
            transaction.Id,
            wallet.Id,
            wallet.UserId,
            wallet.Currency,
            transaction.Amount,
            transaction.BalanceAfter,
            transaction.Reference,
            transaction.CreatedAtUtc);

        var pdfBytes = await _receiptPdfGenerator.GenerateAsync(pdfModel, cancellationToken);
        var objectKey = $"receipts/{wallet.Id}/{transaction.Id}.pdf";

        var result = await _objectStorage.PutObjectAsync(
            objectKey,
            "application/pdf",
            pdfBytes,
            cancellationToken);

        return new ReceiptDto(result.ObjectKey, result.Url);
    }
}
