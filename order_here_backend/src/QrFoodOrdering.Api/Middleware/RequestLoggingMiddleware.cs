using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace QrFoodOrdering.Api.Middleware;

public sealed class RequestLoggingMiddleware : IMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var traceId = context.Response.Headers[TraceIdMiddleware.HeaderName].ToString();
        if (string.IsNullOrWhiteSpace(traceId))
            traceId = context.TraceIdentifier;

        using var scope = _logger.BeginScope(
            new Dictionary<string, object?>
            {
                ["TraceId"] = traceId,
                ["Method"] = context.Request.Method,
                ["Path"] = context.Request.Path.Value,
            }
        );

        _logger.LogInformation(
            "HttpRequestStarted {@data}",
            new
            {
                TraceId = traceId,
                Method = context.Request.Method,
                Path = context.Request.Path.Value,
                QueryString = context.Request.QueryString.Value,
                RemoteIp = context.Connection.RemoteIpAddress?.ToString(),
            }
        );

        var stopwatch = Stopwatch.StartNew();
        await next(context);
        stopwatch.Stop();

        var statusCode = context.Response.StatusCode;
        var level = statusCode >= StatusCodes.Status500InternalServerError
            ? LogLevel.Error
            : statusCode >= StatusCodes.Status400BadRequest
                ? LogLevel.Warning
                : LogLevel.Information;

        _logger.Log(
            level,
            "HttpRequestCompleted {@data}",
            new
            {
                TraceId = traceId,
                Method = context.Request.Method,
                Path = context.Request.Path.Value,
                StatusCode = statusCode,
                DurationMs = stopwatch.ElapsedMilliseconds,
            }
        );
    }
}
