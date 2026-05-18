using Microsoft.AspNetCore.Mvc;

namespace FinWallet.Api.Security;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequireTotpAttribute : TypeFilterAttribute
{
    public RequireTotpAttribute()
        : base(typeof(TotpAuthorizationFilter))
    {
    }
}
