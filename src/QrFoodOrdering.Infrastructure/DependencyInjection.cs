using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Idempotency;
using QrFoodOrdering.Infrastructure.Persistence;
using QrFoodOrdering.Infrastructure.Repositories;
using QrFoodOrdering.Infrastructure.Idempotency;
using QrFoodOrdering.Infrastructure.Audit;

namespace QrFoodOrdering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<QrFoodOrderingDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();
        services.AddSingleton<IAuditLogWriter, FileAuditLogWriter>();

        return services;
    }
}
