using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FinWallet.Api.Security;

public sealed class TotpHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAttribute = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<RequireTotpAttribute>()
            .Any();

        if (!hasAttribute && context.MethodInfo.DeclaringType is not null)
        {
            hasAttribute = context.MethodInfo.DeclaringType
                .GetCustomAttributes(true)
                .OfType<RequireTotpAttribute>()
                .Any();
        }

        if (!hasAttribute)
        {
            return;
        }

        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Totp-Code",
            In = ParameterLocation.Header,
            Required = true,
            Description = "Ma TOTP 6 so tu app xac thuc",
            Schema = new OpenApiSchema { Type = "string" }
        });

        var bearerScheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        };

        var totpScheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Totp"
            }
        };

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                { bearerScheme, Array.Empty<string>() },
                { totpScheme, Array.Empty<string>() }
            }
        };
    }
}
