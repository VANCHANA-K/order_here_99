using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Audit;
using QrFoodOrdering.Application.Common.Idempotency;
using QrFoodOrdering.Application.Tables;
using QrFoodOrdering.Infrastructure.Audit;
using QrFoodOrdering.Infrastructure.Idempotency;
using QrFoodOrdering.Infrastructure.Persistence;
using QrFoodOrdering.Infrastructure.Repositories;

namespace QrFoodOrdering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var auditLogOptions =
            configuration.GetSection(AuditLogOptions.SectionName).Get<AuditLogOptions>()
            ?? new AuditLogOptions();
        AuditLogOptionsValidator.Validate(auditLogOptions);

        services.AddSingleton<IOptions<AuditLogOptions>>(Options.Create(auditLogOptions));

        services.AddDbContext<QrFoodOrderingDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default"))
        );

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ITablesRepository, TablesRepository>();
        services.AddScoped<IQrRepository, QrRepository>();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IIdempotencyStore, DbIdempotencyStore>();
        services.AddSingleton<IAuditLogWriter, FileAuditLogWriter>();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }
}
