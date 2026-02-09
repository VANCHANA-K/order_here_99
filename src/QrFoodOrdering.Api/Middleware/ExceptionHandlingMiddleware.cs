using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QrFoodOrdering.Api.Contracts.Common;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Application.Common.Observability;
using QrFoodOrdering.Domain.Common;

namespace QrFoodOrdering.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITraceContext trace)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            await WriteError(
                context,
                trace.TraceId,
                HttpStatusCode.BadRequest,
                "DOMAIN_RULE_VIOLATION",
                ex.Message,
                ex
            );
        }
        catch (NotFoundException ex)
        {
            await WriteError(
                context,
                trace.TraceId,
                HttpStatusCode.NotFound,
                "RESOURCE_NOT_FOUND",
                ex.Message,
                ex
            );
        }
        catch (InvalidRequestException ex)
        {
            await WriteError(
                context,
                trace.TraceId,
                HttpStatusCode.BadRequest,
                "INVALID_ARGUMENT",
                ex.Message,
                ex
            );
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await WriteError(
                context,
                trace.TraceId,
                HttpStatusCode.Conflict,
                "CONCURRENCY_CONFLICT",
                "The resource was modified by another request.",
                ex
            );
        }
        catch (DbUpdateException ex)
        {
            await WriteError(
                context,
                trace.TraceId,
                HttpStatusCode.ServiceUnavailable,
                "INFRA_FAILURE",
                "A persistence error occurred.",
                ex
            );
        }
        catch (Exception ex)
        {
            await WriteError(
                context,
                trace.TraceId,
                HttpStatusCode.InternalServerError,
                "INTERNAL_ERROR",
                "An unexpected error occurred.",
                ex
            );
        }
    }

    private async Task WriteError(
        HttpContext context,
        string traceId,
        HttpStatusCode status,
        string code,
        string message,
        Exception ex
    )
    {
        _logger.LogError(
            ex,
            "RequestFailed {@data}",
            new
            {
                TraceId = traceId,
                Path = context.Request.Path.Value,
                Method = context.Request.Method,
                StatusCode = (int)status,
                Code = code,
            }
        );

        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = (int)status;

        var payload = new ApiErrorResponse(new ApiError(code, message, traceId));
        await context.Response.WriteAsJsonAsync(payload);
    }
}
