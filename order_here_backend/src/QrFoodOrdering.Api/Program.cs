using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
// API Middleware
using QrFoodOrdering.Api.Contracts.Common;
using QrFoodOrdering.Api.Controllers;
using QrFoodOrdering.Api.Infrastructure;
using QrFoodOrdering.Api.Middleware;
using QrFoodOrdering.Api.Swagger;
using QrFoodOrdering.Api.Validation;
// Application
using QrFoodOrdering.Application;
using QrFoodOrdering.Application.Common.Observability;
using QrFoodOrdering.Application.Qr.Resolve;
using QrFoodOrdering.Domain.Menu;
// Infrastructure
using QrFoodOrdering.Infrastructure;
using QrFoodOrdering.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var runtime = builder.Configuration
    .GetSection(RuntimeEnvironmentOptions.SectionName)
    .Get<RuntimeEnvironmentOptions>()
    ?? new RuntimeEnvironmentOptions();
var connectionString = builder.Configuration.GetConnectionString("Default");
RuntimeEnvironmentValidator.Validate(builder.Environment, runtime, connectionString);

// Controllers
var controllers = builder.Services.AddControllers();
if (!builder.Environment.IsEnvironment("Test"))
{
    controllers.ConfigureApplicationPartManager(manager =>
        manager.FeatureProviders.Add(new ExcludeControllerFeatureProvider(typeof(TestErrorController)))
    );
}

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<ApiErrorResponseExamplesOperationFilter>();
});
builder.Services.AddTransient<ExceptionHandlingMiddleware>();
builder.Services.AddTransient<RequestLoggingMiddleware>();
builder.Services.AddSingleton<IInFlightRequestGate, InFlightRequestGate>();
builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>(
    "database",
    failureStatus: HealthStatus.Unhealthy,
    tags: ["ready"]
);

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

        var (errorCode, message) = ModelValidationErrorMapper.Map(context.ModelState);

        var body = new ApiErrorResponse(errorCode, message, traceId);

        return new BadRequestObjectResult(body);
    };
});

// DI: Application + Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ResolveQrHandler>();

// Observability
builder.Services.AddScoped<ITraceContext, TraceContext>();

// Note: IIdempotencyStore, Audit writer, DbContext are registered inside AddInfrastructure()

var app = builder.Build();

// ----------------------
// Middleware Pipeline
// ----------------------

// 1) TraceId (first)
app.UseMiddleware<TraceIdMiddleware>();

// 2) Request logging with request metadata
app.UseMiddleware<RequestLoggingMiddleware>();

// 3) Global exception handler
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 4) JWT stub (placeholder)
app.UseMiddleware<JwtStubMiddleware>();

// 5) CORS (must be before MapControllers)
if (runtime.EnableFrontendDevCors)
{
    app.UseCors("FrontendDev");
}

// 6) Ensure 404 from unmatched routes returns JSON error
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
                ApiErrorCodes.EndpointNotFound,
                "The requested endpoint was not found.",
                traceId
            );

            await context.Response.WriteAsJsonAsync(payload);
        }
    }
);

// Endpoints
if (runtime.EnableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

if (runtime.ApplyMigrationsOnStartup)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<QrFoodOrderingDbContext>();
    await db.Database.MigrateAsync();

    if (runtime.SeedDemoDataOnStartup && !await db.MenuItems.AnyAsync())
    {
        db.MenuItems.AddRange(
            new MenuItem("M001", "Pad Thai", 60),
            new MenuItem("M002", "Fried Rice", 55),
            new MenuItem("M003", "Iced Tea", 25)
        );

        var unavailable = new MenuItem("M004", "Grilled Pork (Sold out)", 70);
        unavailable.SetAvailability(false);
        db.MenuItems.Add(unavailable);

        await db.SaveChangesAsync();
    }
}

app.Run();

public partial class Program { }
