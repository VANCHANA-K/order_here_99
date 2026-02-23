using System.Net;
using System.Text.Json;
using QrFoodOrdering.Api.Contracts.Common;
using QrFoodOrdering.Application.Common.Exceptions;

namespace QrFoodOrdering.Api.Middleware;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private static Task HandleException(HttpContext ctx, Exception ex)
    {
        var traceId =
            ctx.Response.Headers.TryGetValue("x-trace-id", out var headerTraceId) && !string.IsNullOrWhiteSpace(headerTraceId)
                ? headerTraceId.ToString()
                : ctx.TraceIdentifier;

        (HttpStatusCode status, string errorCode, string message) = ex switch
        {
            InvalidRequestException e => (HttpStatusCode.BadRequest, "VALIDATION_ERROR", e.Message),
            ConflictException e       => (HttpStatusCode.Conflict, e.ErrorCode, e.Message),
            NotFoundException e       => (HttpStatusCode.NotFound, "NOT_FOUND", e.Message),

            _ => (HttpStatusCode.InternalServerError, "UNEXPECTED_ERROR", "Unexpected error occurred."),
        };

        var body = new ApiErrorResponse(errorCode, message, traceId);

        ctx.Response.ContentType = "application/json; charset=utf-8";
        ctx.Response.StatusCode = (int)status;

        return ctx.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }
}
