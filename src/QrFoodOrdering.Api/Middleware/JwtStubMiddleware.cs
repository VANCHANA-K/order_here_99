namespace QrFoodOrdering.Api.Middleware;

public sealed class JwtStubMiddleware
{
    private readonly RequestDelegate _next;

    public JwtStubMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Sprint 0 Day 4:
        // Stub only — no validation
        // Purpose: lock middleware position

        await _next(context);
    }
}
