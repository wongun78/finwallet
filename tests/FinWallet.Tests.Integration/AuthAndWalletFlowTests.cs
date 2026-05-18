using System.Net.Http.Headers;
using System.Net.Http.Json;
using FinWallet.Api.Models.Requests;
using FinWallet.Application.Common.Models;
using FinWallet.Tests.Integration.Testing;
using FluentAssertions;
using OtpNet;
using Xunit;

namespace FinWallet.Tests.Integration;

public sealed class AuthAndWalletFlowTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthAndWalletFlowTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Login_CreateWallet_TopUp_Payment_Refund_Succeeds()
    {
        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest("user1@finwallet.local", "P@ssw0rd!"));

        registerResponse.EnsureSuccessStatusCode();

        var registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthResultDto>();
        registerResult.Should().NotBeNull();
        registerResult!.AccessToken.Should().NotBeNullOrEmpty();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registerResult.AccessToken);

        var totpSetupResponse = await _client.PostAsync("/api/auth/totp/setup", null);
        totpSetupResponse.EnsureSuccessStatusCode();
        var totpSetup = await totpSetupResponse.Content.ReadFromJsonAsync<TotpSetupDto>();
        totpSetup.Should().NotBeNull();

        var totp = new Totp(Base32Encoding.ToBytes(totpSetup!.Secret));
        var totpCode = totp.ComputeTotp();

        var totpConfirmResponse = await _client.PostAsJsonAsync(
            "/api/auth/totp/confirm",
            new ConfirmTotpRequest(totpCode));
        totpConfirmResponse.EnsureSuccessStatusCode();

        var walletResponse = await _client.PostAsJsonAsync(
            "/api/wallets",
            new CreateWalletRequest("VND"));

        walletResponse.EnsureSuccessStatusCode();
        var wallet = await walletResponse.Content.ReadFromJsonAsync<WalletDto>();
        wallet.Should().NotBeNull();

        var topupResponse = await _client.PostAsJsonAsync(
            $"/api/wallets/{wallet!.WalletId}/topup",
            new TopUpWalletRequest(100_000m, "TOPUP_TEST"));

        topupResponse.EnsureSuccessStatusCode();

        _client.DefaultRequestHeaders.Remove("X-Totp-Code");
        _client.DefaultRequestHeaders.Add("X-Totp-Code", totp.ComputeTotp());

        var paymentResponse = await _client.PostAsJsonAsync(
            $"/api/wallets/{wallet.WalletId}/payment",
            new PaymentRequest(20_000m, "PAYMENT_TEST"));

        paymentResponse.EnsureSuccessStatusCode();

        _client.DefaultRequestHeaders.Remove("X-Totp-Code");
        _client.DefaultRequestHeaders.Add("X-Totp-Code", totp.ComputeTotp());

        var refundResponse = await _client.PostAsJsonAsync(
            $"/api/wallets/{wallet.WalletId}/refund",
            new RefundRequest(5_000m, "REFUND_TEST"));

        refundResponse.EnsureSuccessStatusCode();

        var getWalletResponse = await _client.GetAsync($"/api/wallets/{wallet.WalletId}");
        getWalletResponse.EnsureSuccessStatusCode();

        var walletAfter = await getWalletResponse.Content.ReadFromJsonAsync<WalletDto>();
        walletAfter!.Balance.Should().Be(85_000m);
    }
}
