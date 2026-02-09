using Microsoft.AspNetCore.Mvc;
// API Middleware
using QrFoodOrdering.Api.Contracts.Common;
using QrFoodOrdering.Api.Middleware;
// Application
using QrFoodOrdering.Application;
using QrFoodOrdering.Application.Common.Observability;
// Infrastructure
using QrFoodOrdering.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Validation: unify 400 response shape
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var traceId = context.HttpContext.Response.Headers["X-Trace-Id"].ToString();
        if (string.IsNullOrWhiteSpace(traceId))
            traceId = context.HttpContext.TraceIdentifier;

        var firstError = context
            .ModelState.SelectMany(kvp => kvp.Value!.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault();

        var message = string.IsNullOrWhiteSpace(firstError) ? "Invalid request." : firstError;

        var body = new ApiErrorResponse(new ApiError("INVALID_ARGUMENT", message, traceId));

        return new BadRequestObjectResult(body);
    };
});

// DI: Application + Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Observability
builder.Services.AddScoped<ITraceContext, TraceContext>();

// Note: IIdempotencyStore, Audit writer, DbContext are registered inside AddInfrastructure()

var app = builder.Build();

// Middleware
// 1) TraceId (first)
app.UseMiddleware<TraceIdMiddleware>();

// 2) Global exception handler
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 3) JWT stub (placeholder)
app.UseMiddleware<JwtStubMiddleware>();

// 4) Ensure 404 from unmatched routes returns JSON error
app.Use(
    async (context, next) =>
    {
        await next();

        if (context.Response.HasStarted)
            return;

        // If no endpoint matched and it's a 404, return our error envelope
        if (
            context.GetEndpoint() is null
            && context.Response.StatusCode == StatusCodes.Status404NotFound
        )
        {
            var traceId = context.Response.Headers["X-Trace-Id"].ToString();
            if (string.IsNullOrWhiteSpace(traceId))
                traceId = context.TraceIdentifier;

            var payload = new ApiErrorResponse(
                new ApiError("ENDPOINT_NOT_FOUND", "The requested endpoint was not found.", traceId)
            );

            await context.Response.WriteAsJsonAsync(payload);
        }
    }
);

// Endpoints
app.MapControllers();

app.Run();
