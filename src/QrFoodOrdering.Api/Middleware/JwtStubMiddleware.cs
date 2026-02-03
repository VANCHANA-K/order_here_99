namespace QrFoodOrdering.Api.Middleware;

public sealed class JwtStubMiddleware
{
    private readonly RequestDelegate _next;

    public JwtStubMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Stub only — no auth logic yet
        await _next(context);
    }
}

