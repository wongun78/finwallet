namespace FinWallet.Application.Common.Interfaces;

public interface IWebhookSender
{
    Task SendAsync<T>(T payload, CancellationToken ct) where T : class;
}
