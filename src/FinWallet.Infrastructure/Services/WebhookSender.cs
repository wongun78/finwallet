using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Options;
using Microsoft.Extensions.Options;

namespace FinWallet.Infrastructure.Services;

public sealed class WebhookSender : IWebhookSender
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WebhookOptions _options;

    public WebhookSender(IHttpClientFactory httpClientFactory, IOptions<WebhookOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task SendAsync<T>(T payload, CancellationToken ct) where T : class
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            return;
        }

        var json = JsonSerializer.Serialize(payload);
        var signature = BuildSignature(json, _options.Secret);

        var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Signature", signature);

        var client = _httpClientFactory.CreateClient();
        var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }

    private static string BuildSignature(string body, string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            return string.Empty;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
        return Convert.ToBase64String(hash);
    }
}
