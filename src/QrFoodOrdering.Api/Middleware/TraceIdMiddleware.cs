using Microsoft.AspNetCore.Http;
using QrFoodOrdering.Application.Common.Observability;

namespace QrFoodOrdering.Api.Middleware;

public sealed class TraceIdMiddleware
{
    public const string HeaderName = "X-Trace-Id";

    private readonly RequestDelegate _next;

    public TraceIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITraceContext traceContext)
    {
        // client can pass trace id; if not, generate new
        var incoming = context.Request.Headers[HeaderName].FirstOrDefault();
        var traceId = string.IsNullOrWhiteSpace(incoming)
            ? Guid.NewGuid().ToString("N")
            : incoming!;

        // attach to response for every request
        context.Response.Headers[HeaderName] = traceId;

        // attach to application-friendly trace context
        if (traceContext is TraceContext tc)
            tc.TraceId = traceId;

        await _next(context);
    }
}
