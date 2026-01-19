namespace QrFoodOrdering.Api.Middleware;

public sealed class TraceIdMiddleware
{
    private const string HeaderName = "X-Trace-Id";
    private readonly RequestDelegate _next;

    public TraceIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var traceId))
        {
            traceId = Guid.NewGuid().ToString("N");
            context.Request.Headers[HeaderName] = traceId;
        }

        context.Response.Headers[HeaderName] = traceId;
        context.TraceIdentifier = traceId!;

        await _next(context);
    }
}
