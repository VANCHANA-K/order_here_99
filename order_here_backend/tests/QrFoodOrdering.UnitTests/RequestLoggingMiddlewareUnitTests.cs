using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QrFoodOrdering.Api.Middleware;

namespace QrFoodOrdering.UnitTests;

public sealed class RequestLoggingMiddlewareUnitTests
{
    [Fact]
    public async Task InvokeAsync_should_log_start_and_completion_with_request_metadata()
    {
        var logger = new TestLogger<RequestLoggingMiddleware>();
        var middleware = new RequestLoggingMiddleware(logger);
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/v1/orders";
        context.Request.QueryString = new QueryString("?include=items");
        context.TraceIdentifier = "trace-123";
        context.Response.Headers[TraceIdMiddleware.HeaderName] = "trace-123";

        await middleware.InvokeAsync(
            context,
            ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status201Created;
                return Task.CompletedTask;
            }
        );

        Assert.Equal(2, logger.Entries.Count);

        var started = logger.Entries[0];
        Assert.Equal(LogLevel.Information, started.Level);
        Assert.Contains("HttpRequestStarted", started.Message, StringComparison.Ordinal);
        Assert.Contains("trace-123", started.StateText, StringComparison.Ordinal);
        Assert.Contains("/api/v1/orders", started.StateText, StringComparison.Ordinal);
        Assert.Contains("include=items", started.StateText, StringComparison.Ordinal);

        var completed = logger.Entries[1];
        Assert.Equal(LogLevel.Information, completed.Level);
        Assert.Contains("HttpRequestCompleted", completed.Message, StringComparison.Ordinal);
        Assert.Contains("trace-123", completed.StateText, StringComparison.Ordinal);
        Assert.Contains("/api/v1/orders", completed.StateText, StringComparison.Ordinal);
        Assert.Contains("201", completed.StateText, StringComparison.Ordinal);
        Assert.Contains("DurationMs", completed.StateText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvokeAsync_should_log_warning_for_client_error_status_codes()
    {
        var logger = new TestLogger<RequestLoggingMiddleware>();
        var middleware = new RequestLoggingMiddleware(logger);
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/v1/orders/missing";
        context.TraceIdentifier = "trace-404";
        context.Response.Headers[TraceIdMiddleware.HeaderName] = "trace-404";

        await middleware.InvokeAsync(
            context,
            ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status404NotFound;
                return Task.CompletedTask;
            }
        );

        Assert.Equal(LogLevel.Warning, logger.Entries[1].Level);
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new LogEntry(logLevel, formatter(state, exception), state?.ToString() ?? string.Empty));
        }

        public sealed record LogEntry(LogLevel Level, string Message, string StateText);

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose() { }
        }
    }
}
