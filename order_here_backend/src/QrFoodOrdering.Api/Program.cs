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

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

// CORS (Frontend Dev)
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "FrontendDev",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:3000") // Next.js dev server
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

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

        var body = new ApiErrorResponse("VALIDATION_ERROR", message, traceId);

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

// ----------------------
// Middleware Pipeline
// ----------------------

// 1) TraceId (first)
app.UseMiddleware<TraceIdMiddleware>();

// 2) Global exception handler
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 3) JWT stub (placeholder)
app.UseMiddleware<JwtStubMiddleware>();

// 4) CORS (must be before MapControllers)
app.UseCors("FrontendDev");

// 5) Ensure 404 from unmatched routes returns JSON error
app.Use(
    async (context, next) =>
    {
        await next();

        if (context.Response.HasStarted)
            return;

        if (
            context.GetEndpoint() is null
            && context.Response.StatusCode == StatusCodes.Status404NotFound
        )
        {
            var traceId = context.Response.Headers["X-Trace-Id"].ToString();
            if (string.IsNullOrWhiteSpace(traceId))
                traceId = context.TraceIdentifier;

            var payload = new ApiErrorResponse(
                "ENDPOINT_NOT_FOUND",
                "The requested endpoint was not found.",
                traceId
            );

            await context.Response.WriteAsJsonAsync(payload);
        }
    }
);

// Endpoints
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
