using FinWallet.Application.Common.Interfaces;
using FinWallet.Application.Common.Options;
using Microsoft.Extensions.Options;

namespace FinWallet.Api.Middleware;

public sealed class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimitService _rateLimitService;
    private readonly RateLimitingOptions _options;

    public RateLimitMiddleware(
        RequestDelegate next,
        IRateLimitService rateLimitService,
        IOptions<RateLimitingOptions> options)
    {
        _next = next;
        _rateLimitService = rateLimitService;
        _options = options.Value;
    }

    public async Task Invoke(HttpContext context, ICurrentUserService currentUser)
    {
        if (!_options.Enabled)
        {
            await _next(context);
            return;
        }

        var userId = currentUser.GetUserId();
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = userId is null ? $"ip:{ip}" : $"user:{userId}";

        var allowed = await _rateLimitService.IsRequestAllowedAsync(
            $"rl:{key}",
            _options.RequestsPerMinute,
            TimeSpan.FromMinutes(1),
            context.RequestAborted);

        if (!allowed)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsJsonAsync(new { message = "Vuot qua gioi han yeu cau." });
            return;
        }

        await _next(context);
    }
}
