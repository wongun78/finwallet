using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FinWallet.Api.Support;

public sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAllowAnonymous = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AllowAnonymousAttribute>()
            .Any();

        if (hasAllowAnonymous)
        {
            return;
        }

        var hasAuthorize = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>()
            .Any();

        if (!hasAuthorize && context.MethodInfo.DeclaringType is not null)
        {
            hasAuthorize = context.MethodInfo.DeclaringType
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Any();
        }

        if (!hasAuthorize)
        {
            return;
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();

        var bearerScheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        };

        if (operation.Security.Any(requirement => requirement.Keys.Any(key => key.Reference?.Id == "Bearer")))
        {
            return;
        }

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            { bearerScheme, Array.Empty<string>() }
        });
    }
}
