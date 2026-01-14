using QrFoodOrdering.Application.Orders;
using QrFoodOrdering.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


// 🔒 Swagger is intentionally disabled for Sprint 0 Day 1
// Tooling incompatibility with .NET 9 is expected and acceptable at this stage
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// Application registrations (Sprint 0: simple DI)
builder.Services.AddScoped<CreateOrder>();

// Infrastructure registrations (placeholder)
builder.Services.AddInfrastructure();

var app = builder.Build();

// Swagger setup
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.MapControllers();

// Sprint 0 Day1: ยังไม่ทำ /health (จะทำ Day4 ตามแผน)
app.Run();
