using QrFoodOrdering.Api.Middleware;
using QrFoodOrdering.Application.Orders;
using QrFoodOrdering.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


// 🔒 Swagger is intentionally disabled for Sprint 0
// Reason:
// - Not required for API contract locking
// - Avoid tooling noise during foundation sprint
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// Application layer (use-case level)
builder.Services.AddScoped<CreateOrder>();

// Infrastructure layer (DbContext, persistence)
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("Default")!
);


var app = builder.Build();

//
// Middleware pipeline
//

// ❗ Global exception handler — MUST be first
// Purpose:
// - Catch all unhandled exceptions
// - Prevent stack trace leakage
// - Standardize error response
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 🔐 JWT middleware stub (position locked, no auth logic yet)
// Purpose:
// - Define pipeline position only
// - Actual JWT validation will be added in later sprint
app.UseMiddleware<JwtStubMiddleware>();

// Routing to controllers
app.MapControllers();

// Application entry point
app.Run();
