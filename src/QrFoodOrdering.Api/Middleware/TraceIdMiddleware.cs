namespace QrFoodOrdering.Api.Middleware;
public sealed class TraceIdMiddleware
{
    private const string HeaderName = "X-Trace-Id";
    private readonly RequestDelegate _next;

    public TraceIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Guid.NewGuid().ToString("N");
        context.Response.Headers[HeaderName] = traceId;
        context.Items[HeaderName] = traceId;

        await _next(context);
    }
}
