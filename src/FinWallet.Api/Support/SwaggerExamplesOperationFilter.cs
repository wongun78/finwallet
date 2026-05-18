using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FinWallet.Api.Support;

public sealed class SwaggerExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath?.ToLowerInvariant() ?? string.Empty;
        var method = context.ApiDescription.HttpMethod?.ToUpperInvariant() ?? string.Empty;

        if (method == "POST" && path == "api/auth/register")
        {
            SetRequestExample(operation, new OpenApiObject
            {
                ["email"] = new OpenApiString("admin@finwallet.local"),
                ["password"] = new OpenApiString("P@ssw0rd!123")
            });
            SetResponseExample(operation, BuildAuthResultExample());
            return;
        }

        if (method == "POST" && path == "api/auth/login")
        {
            SetRequestExample(operation, new OpenApiObject
            {
                ["email"] = new OpenApiString("admin@finwallet.local"),
                ["password"] = new OpenApiString("P@ssw0rd!123"),
                ["totpCode"] = new OpenApiString("123456")
            });
            SetResponseExample(operation, BuildAuthResultExample());
            return;
        }

        if (method == "POST" && path == "api/auth/totp/confirm")
        {
            SetRequestExample(operation, new OpenApiObject
            {
                ["code"] = new OpenApiString("123456")
            });
            SetResponseExample(operation, new OpenApiBoolean(true));
            return;
        }

        if (method == "POST" && path == "api/auth/totp/setup")
        {
            SetResponseExample(operation, new OpenApiObject
            {
                ["secret"] = new OpenApiString("JBSWY3DPEHPK3PXP"),
                ["otpAuthUri"] = new OpenApiString("otpauth://totp/FinWallet:admin@finwallet.local?secret=JBSWY3DPEHPK3PXP&issuer=FinWallet")
            });
            return;
        }

        if (method == "POST" && path == "api/auth/refresh")
        {
            SetRequestExample(operation, new OpenApiObject
            {
                ["refreshToken"] = new OpenApiString("f9b2a8c1d0e84f0d9f6b2f3d5e0a1234")
            });
            SetResponseExample(operation, BuildAuthResultExample());
            return;
        }

        if (method == "POST" && path == "api/auth/logout")
        {
            SetRequestExample(operation, new OpenApiObject
            {
                ["refreshToken"] = new OpenApiString("f9b2a8c1d0e84f0d9f6b2f3d5e0a1234")
            });
            SetResponseExample(operation, new OpenApiBoolean(true));
            return;
        }

        if (method == "POST" && path == "api/wallets")
        {
            SetRequestExample(operation, new OpenApiObject
            {
                ["currency"] = new OpenApiString("VND")
            });
            SetResponseExample(operation, BuildWalletExample());
            return;
        }

        if (method == "POST" && path == "api/wallets/transfer")
        {
            SetRequestExample(operation, new OpenApiObject
            {
                ["fromWalletId"] = new OpenApiString("38654a82-cf29-4ad7-9276-0ffb2bc9ce4e"),
                ["toWalletId"] = new OpenApiString("cc6b32af-dc52-44b9-ba95-c1594cba6af2"),
                ["amount"] = new OpenApiDouble(250000),
                ["reference"] = new OpenApiString("TRANSFER-INV-2026-05")
            });

            var response = new OpenApiArray
            {
                BuildTransactionExample(
                    "d4613f44-1f3a-4fc1-ac48-48573f916dc0",
                    "38654a82-cf29-4ad7-9276-0ffb2bc9ce4e",
                    "TransferOut",
                    "Completed",
                    250000,
                    1200000,
                    950000,
                    "TRANSFER-INV-2026-05",
                    "cc6b32af-dc52-44b9-ba95-c1594cba6af2"),
                BuildTransactionExample(
                    "54eae7e1-6486-4ab2-a27f-01e33d22a5c9",
                    "cc6b32af-dc52-44b9-ba95-c1594cba6af2",
                    "TransferIn",
                    "Completed",
                    250000,
                    300000,
                    550000,
                    "TRANSFER-INV-2026-05",
                    "38654a82-cf29-4ad7-9276-0ffb2bc9ce4e")
            };
            SetResponseExample(operation, response);
            return;
        }

        if (method == "POST" && path.EndsWith("/topup", StringComparison.OrdinalIgnoreCase))
        {
            SetRequestExample(operation, new OpenApiObject
            {
                ["amount"] = new OpenApiDouble(500000),
                ["reference"] = new OpenApiString("TOPUP-2026-05")
            });
            SetResponseExample(operation, BuildTransactionExample(
                "9dc88b0c-44a1-430d-a5c9-b2ed697340db",
                "38654a82-cf29-4ad7-9276-0ffb2bc9ce4e",
                "TopUp",
                "Completed",
                500000,
                450000,
                950000,
                "TOPUP-2026-05",
                null));
            return;
        }

        if (method == "POST" && path.EndsWith("/payment", StringComparison.OrdinalIgnoreCase))
        {
            SetRequestExample(operation, new OpenApiObject
            {
                ["amount"] = new OpenApiDouble(120000),
                ["reference"] = new OpenApiString("PAY-ORDER-2026-05")
            });
            SetResponseExample(operation, BuildTransactionExample(
                "4d36a6b0-d1b7-4048-bdf5-86086c2f4348",
                "38654a82-cf29-4ad7-9276-0ffb2bc9ce4e",
                "Payment",
                "Completed",
                120000,
                950000,
                830000,
                "PAY-ORDER-2026-05",
                null));
            return;
        }

        if (method == "POST" && path.EndsWith("/refund", StringComparison.OrdinalIgnoreCase))
        {
            SetRequestExample(operation, new OpenApiObject
            {
                ["amount"] = new OpenApiDouble(120000),
                ["reference"] = new OpenApiString("REFUND-2026-05")
            });
            SetResponseExample(operation, BuildTransactionExample(
                "cc6b32af-dc52-44b9-ba95-c1594cba6af2",
                "38654a82-cf29-4ad7-9276-0ffb2bc9ce4e",
                "Refund",
                "Completed",
                120000,
                830000,
                950000,
                "REFUND-2026-05",
                null));
            return;
        }

        if (method == "POST" && path.EndsWith("/receipt", StringComparison.OrdinalIgnoreCase))
        {
            SetResponseExample(operation, new OpenApiObject
            {
                ["objectKey"] = new OpenApiString("receipts/2026/05/receipt-9dc88b0c.pdf"),
                ["url"] = new OpenApiString("https://minio.finwallet.local/finwallet-receipts/receipts/2026/05/receipt-9dc88b0c.pdf")
            });
            return;
        }
    }

    private static OpenApiObject BuildAuthResultExample()
    {
        return new OpenApiObject
        {
            ["userId"] = new OpenApiString("4d36a6b0-d1b7-4048-bdf5-86086c2f4348"),
            ["email"] = new OpenApiString("admin@finwallet.local"),
            ["accessToken"] = new OpenApiString("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."),
            ["expiresAtUtc"] = new OpenApiDateTime(new DateTime(2026, 5, 12, 9, 30, 0, DateTimeKind.Utc)),
            ["requiresTotp"] = new OpenApiBoolean(false),
            ["refreshToken"] = new OpenApiString("f9b2a8c1d0e84f0d9f6b2f3d5e0a1234"),
            ["refreshExpiresAtUtc"] = new OpenApiDateTime(new DateTime(2026, 5, 19, 9, 30, 0, DateTimeKind.Utc))
        };
    }

    private static OpenApiObject BuildWalletExample()
    {
        return new OpenApiObject
        {
            ["walletId"] = new OpenApiString("38654a82-cf29-4ad7-9276-0ffb2bc9ce4e"),
            ["userId"] = new OpenApiString("4d36a6b0-d1b7-4048-bdf5-86086c2f4348"),
            ["currency"] = new OpenApiString("VND"),
            ["balance"] = new OpenApiDouble(950000),
            ["status"] = new OpenApiString("Active"),
            ["createdAtUtc"] = new OpenApiDateTime(new DateTime(2026, 5, 12, 8, 0, 0, DateTimeKind.Utc)),
            ["updatedAtUtc"] = new OpenApiDateTime(new DateTime(2026, 5, 12, 8, 30, 0, DateTimeKind.Utc))
        };
    }

    private static OpenApiObject BuildTransactionExample(
        string transactionId,
        string walletId,
        string type,
        string status,
        double amount,
        double balanceBefore,
        double balanceAfter,
        string reference,
        string? counterpartyWalletId)
    {
        var obj = new OpenApiObject
        {
            ["transactionId"] = new OpenApiString(transactionId),
            ["walletId"] = new OpenApiString(walletId),
            ["type"] = new OpenApiString(type),
            ["status"] = new OpenApiString(status),
            ["amount"] = new OpenApiDouble(amount),
            ["balanceBefore"] = new OpenApiDouble(balanceBefore),
            ["balanceAfter"] = new OpenApiDouble(balanceAfter),
            ["reference"] = new OpenApiString(reference),
            ["createdAtUtc"] = new OpenApiDateTime(new DateTime(2026, 5, 12, 8, 30, 0, DateTimeKind.Utc))
        };

        if (!string.IsNullOrWhiteSpace(counterpartyWalletId))
        {
            obj["counterpartyWalletId"] = new OpenApiString(counterpartyWalletId);
        }
        else
        {
            obj["counterpartyWalletId"] = new OpenApiNull();
        }

        return obj;
    }

    private static void SetRequestExample(OpenApiOperation operation, IOpenApiAny example)
    {
        if (operation.RequestBody?.Content is null)
        {
            return;
        }

        if (operation.RequestBody.Content.TryGetValue("application/json", out var mediaType))
        {
            mediaType.Example = example;
        }
    }

    private static void SetResponseExample(OpenApiOperation operation, IOpenApiAny example)
    {
        if (!operation.Responses.TryGetValue("200", out var response))
        {
            response = new OpenApiResponse { Description = "OK" };
            operation.Responses["200"] = response;
        }

        response.Content ??= new Dictionary<string, OpenApiMediaType>();
        if (!response.Content.TryGetValue("application/json", out var mediaType))
        {
            mediaType = new OpenApiMediaType();
            response.Content["application/json"] = mediaType;
        }

        mediaType.Example = example;
    }
}
