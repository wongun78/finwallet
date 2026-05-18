using System.Net;
using System.Text.Json;
using FinWallet.Application.Common.Exceptions;
using FinWallet.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FinWallet.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppValidationException ex)
        {
            await WriteProblemDetails(context, (int)HttpStatusCode.BadRequest, "Validation failed", ex.Message, ex.Errors);
        }
        catch (NotFoundException ex)
        {
            await WriteProblemDetails(context, (int)HttpStatusCode.NotFound, "Not found", ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            await WriteProblemDetails(context, (int)HttpStatusCode.Unauthorized, "Unauthorized", ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await WriteProblemDetails(context, (int)HttpStatusCode.Forbidden, "Forbidden", ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteProblemDetails(context, (int)HttpStatusCode.Conflict, "Conflict", ex.Message);
        }
        catch (DomainException ex)
        {
            await WriteProblemDetails(context, (int)HttpStatusCode.BadRequest, "Domain error", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblemDetails(context, (int)HttpStatusCode.InternalServerError, "Server error", ex.ToString());
        }
    }

    private static async Task WriteProblemDetails(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        IDictionary<string, string[]>? errors = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        ProblemDetails problem;
        if (errors is not null)
        {
            problem = new ValidationProblemDetails(errors)
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            };
        }
        else
        {
            problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            };
        }

        var json = JsonSerializer.Serialize(problem);
        await context.Response.WriteAsync(json);
    }
}
