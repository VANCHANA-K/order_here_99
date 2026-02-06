using Microsoft.AspNetCore.Mvc;
// API Middleware
using QrFoodOrdering.Api.Contracts.Common;
using QrFoodOrdering.Api.Middleware;
// Application
using QrFoodOrdering.Application;
using QrFoodOrdering.Application.Common.Idempotency;
// Infrastructure
using QrFoodOrdering.Infrastructure;
// using QrFoodOrdering.Infrastructure.Idempotency;



var builder = WebApplication.CreateBuilder(args);

//
// Controllers
//
builder.Services.AddControllers();

//
// ✅ Override ApiController validation response (400)
//
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var traceId = context.HttpContext.Response.Headers["X-Trace-Id"].ToString();
        if (string.IsNullOrWhiteSpace(traceId))
            traceId = context.HttpContext.TraceIdentifier;

        var firstError = context.ModelState
            .SelectMany(kvp => kvp.Value!.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault();

        var message = string.IsNullOrWhiteSpace(firstError)
            ? "Invalid request."
            : firstError;

        var body = new ApiErrorResponse(
            new ApiError(
                "INVALID_ARGUMENT",
                message,
                traceId
            )
        );

        return new BadRequestObjectResult(body);
    };
});

//
// Application & Infrastructure DI
//
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);


// IIdempotencyStore registered in Infrastructure.AddInfrastructure()

var app = builder.Build();

//
// Middleware pipeline (ลำดับสำคัญมาก)
//

// 1️⃣ TraceId — ต้องมาก่อนสุด
app.UseMiddleware<TraceIdMiddleware>();

// 2️⃣ Global exception handler
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 3️⃣ JWT stub (ยังไม่ validate จริง)
app.UseMiddleware<JwtStubMiddleware>();

// Routing
app.MapControllers();

app.Run();
