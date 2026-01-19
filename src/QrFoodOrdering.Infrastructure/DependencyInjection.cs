using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QrFoodOrdering.Infrastructure.Persistence;
using QrFoodOrdering.Infrastructure.Audit;

namespace QrFoodOrdering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddSingleton<IAuditLogWriter, FileAuditLogWriter>();

        return services;
    }
}



