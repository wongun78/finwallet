using FinWallet.Api.Models.Requests;
using FinWallet.Api.Security;
using FinWallet.Application.Common.Models;
using FinWallet.Application.Wallets.Commands;
using FinWallet.Application.Wallets.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinWallet.Api.Controllers;

/// <summary>
/// Nhom API vi dien tu va giao dich.
/// </summary>
[ApiController]
[Authorize]
[Route("api/wallets")]
public sealed class WalletsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WalletsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Tao vi moi cho user dang nhap.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// <code>
    /// { "currency": "VND" }
    /// </code>
    /// Sample response:
    /// <code>
    /// {
    ///   "walletId": "38654a82-cf29-4ad7-9276-0ffb2bc9ce4e",
    ///   "userId": "4d36a6b0-d1b7-4048-bdf5-86086c2f4348",
    ///   "currency": "VND",
    ///   "balance": 950000,
    ///   "status": "Active",
    ///   "createdAtUtc": "2026-05-12T08:00:00Z",
    ///   "updatedAtUtc": "2026-05-12T08:30:00Z"
    /// }
    /// </code>
    /// </remarks>
    [HttpPost]
    public async Task<ActionResult<WalletDto>> CreateWallet(CreateWalletRequest request, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new CreateWalletCommand(request.Currency), ct);
        return Ok(result);
    }

    /// <summary>
    /// Lay danh sach vi cua user dang nhap.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WalletDto>>> GetWallets(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetUserWalletsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Lay chi tiet vi theo id.
    /// </summary>
    [HttpGet("{walletId:guid}")]
    public async Task<ActionResult<WalletDto>> GetWallet(Guid walletId, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetWalletByIdQuery(walletId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Nap tien vao vi.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// <code>
    /// { "amount": 500000, "reference": "TOPUP-2026-05" }
    /// </code>
    /// Sample response:
    /// <code>
    /// {
    ///   "transactionId": "9dc88b0c-44a1-430d-a5c9-b2ed697340db",
    ///   "walletId": "38654a82-cf29-4ad7-9276-0ffb2bc9ce4e",
    ///   "type": "TopUp",
    ///   "status": "Completed",
    ///   "amount": 500000,
    ///   "balanceBefore": 450000,
    ///   "balanceAfter": 950000,
    ///   "reference": "TOPUP-2026-05",
    ///   "counterpartyWalletId": null,
    ///   "createdAtUtc": "2026-05-12T08:30:00Z"
    /// }
    /// </code>
    /// </remarks>
    [HttpPost("{walletId:guid}/topup")]
    public async Task<ActionResult<TransactionDto>> TopUp(Guid walletId, TopUpWalletRequest request, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new TopUpWalletCommand(walletId, request.Amount, request.Reference), ct);
        return Ok(result);
    }

    /// <summary>
    /// Thanh toan tu vi (yeu cau TOTP).
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// <code>
    /// { "amount": 120000, "reference": "PAY-ORDER-2026-05" }
    /// </code>
    /// Sample response:
    /// <code>
    /// {
    ///   "transactionId": "4d36a6b0-d1b7-4048-bdf5-86086c2f4348",
    ///   "walletId": "38654a82-cf29-4ad7-9276-0ffb2bc9ce4e",
    ///   "type": "Payment",
    ///   "status": "Completed",
    ///   "amount": 120000,
    ///   "balanceBefore": 950000,
    ///   "balanceAfter": 830000,
    ///   "reference": "PAY-ORDER-2026-05",
    ///   "counterpartyWalletId": null,
    ///   "createdAtUtc": "2026-05-12T08:30:00Z"
    /// }
    /// </code>
    /// </remarks>
    [HttpPost("{walletId:guid}/payment")]
    [RequireTotp]
    public async Task<ActionResult<TransactionDto>> Payment(Guid walletId, PaymentRequest request, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new PayWalletCommand(walletId, request.Amount, request.Reference), ct);
        return Ok(result);
    }

    /// <summary>
    /// Hoan tien vao vi (yeu cau TOTP).
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// <code>
    /// { "amount": 120000, "reference": "REFUND-2026-05" }
    /// </code>
    /// Sample response:
    /// <code>
    /// {
    ///   "transactionId": "cc6b32af-dc52-44b9-ba95-c1594cba6af2",
    ///   "walletId": "38654a82-cf29-4ad7-9276-0ffb2bc9ce4e",
    ///   "type": "Refund",
    ///   "status": "Completed",
    ///   "amount": 120000,
    ///   "balanceBefore": 830000,
    ///   "balanceAfter": 950000,
    ///   "reference": "REFUND-2026-05",
    ///   "counterpartyWalletId": null,
    ///   "createdAtUtc": "2026-05-12T08:30:00Z"
    /// }
    /// </code>
    /// </remarks>
    [HttpPost("{walletId:guid}/refund")]
    [RequireTotp]
    public async Task<ActionResult<TransactionDto>> Refund(Guid walletId, RefundRequest request, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new RefundWalletCommand(walletId, request.Amount, request.Reference), ct);
        return Ok(result);
    }

    /// <summary>
    /// Chuyen tien giua hai vi (yeu cau TOTP).
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// <code>
    /// {
    ///   "fromWalletId": "38654a82-cf29-4ad7-9276-0ffb2bc9ce4e",
    ///   "toWalletId": "cc6b32af-dc52-44b9-ba95-c1594cba6af2",
    ///   "amount": 250000,
    ///   "reference": "TRANSFER-INV-2026-05"
    /// }
    /// </code>
    /// Sample response:
    /// <code>
    /// [
    ///   {
    ///     "transactionId": "d4613f44-1f3a-4fc1-ac48-48573f916dc0",
    ///     "walletId": "38654a82-cf29-4ad7-9276-0ffb2bc9ce4e",
    ///     "type": "TransferOut",
    ///     "status": "Completed",
    ///     "amount": 250000,
    ///     "balanceBefore": 1200000,
    ///     "balanceAfter": 950000,
    ///     "reference": "TRANSFER-INV-2026-05",
    ///     "counterpartyWalletId": "cc6b32af-dc52-44b9-ba95-c1594cba6af2",
    ///     "createdAtUtc": "2026-05-12T08:30:00Z"
    ///   },
    ///   {
    ///     "transactionId": "54eae7e1-6486-4ab2-a27f-01e33d22a5c9",
    ///     "walletId": "cc6b32af-dc52-44b9-ba95-c1594cba6af2",
    ///     "type": "TransferIn",
    ///     "status": "Completed",
    ///     "amount": 250000,
    ///     "balanceBefore": 300000,
    ///     "balanceAfter": 550000,
    ///     "reference": "TRANSFER-INV-2026-05",
    ///     "counterpartyWalletId": "38654a82-cf29-4ad7-9276-0ffb2bc9ce4e",
    ///     "createdAtUtc": "2026-05-12T08:30:00Z"
    ///   }
    /// ]
    /// </code>
    /// </remarks>
    [HttpPost("transfer")]
    [RequireTotp]
    public async Task<ActionResult<IReadOnlyList<TransactionDto>>> Transfer(TransferRequest request, CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new TransferCommand(request.FromWalletId, request.ToWalletId, request.Amount, request.Reference),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Lay lich su giao dich cua vi.
    /// </summary>
    [HttpGet("{walletId:guid}/transactions")]
    public async Task<ActionResult<IReadOnlyList<TransactionDto>>> GetTransactions(
        Guid walletId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetWalletTransactionsQuery(walletId, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>
    /// Export giao dich ra file Excel (yeu cau TOTP).
    /// </summary>
    [HttpGet("{walletId:guid}/transactions/export")]
    [RequireTotp]
    public async Task<IActionResult> ExportTransactions(Guid walletId, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ExportWalletTransactionsQuery(walletId), ct);
        return File(result.Content, result.ContentType, result.FileName);
    }

    /// <summary>
    /// Tao bien lai PDF va upload len MinIO.
    /// </summary>
    /// <remarks>
    /// Sample response:
    /// <code>
    /// {
    ///   "objectKey": "receipts/2026/05/receipt-9dc88b0c.pdf",
    ///   "url": "https://minio.finwallet.local/finwallet-receipts/receipts/2026/05/receipt-9dc88b0c.pdf"
    /// }
    /// </code>
    /// </remarks>
    [HttpPost("{walletId:guid}/transactions/{transactionId:guid}/receipt")]
    public async Task<ActionResult<ReceiptDto>> GenerateReceipt(
        Guid walletId,
        Guid transactionId,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GenerateReceiptCommand(walletId, transactionId), ct);
        return Ok(result);
    }
}
