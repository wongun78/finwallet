using FinWallet.Application.Transactions.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using FinWallet.Application.Common.Interfaces;

namespace FinWallet.Infrastructure.Services;

public sealed class TransactionCreatedConsumer : IConsumer<TransactionCreatedIntegrationEvent>
{
    private readonly ILogger<TransactionCreatedConsumer> _logger;
    private readonly IWebhookSender _webhookSender;

    public TransactionCreatedConsumer(ILogger<TransactionCreatedConsumer> logger, IWebhookSender webhookSender)
    {
        _logger = logger;
        _webhookSender = webhookSender;
    }

    public async Task Consume(ConsumeContext<TransactionCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        // Ghi chu: xu ly event sau khi giao dich hoan tat.
        _logger.LogInformation(
            "TransactionCreated {TransactionId} Wallet {WalletId} Amount {Amount}",
            message.TransactionId,
            message.WalletId,
            message.Amount);

        // Ghi chu: gui webhook neu co cau hinh.
        await _webhookSender.SendAsync(message, context.CancellationToken);
    }
}
