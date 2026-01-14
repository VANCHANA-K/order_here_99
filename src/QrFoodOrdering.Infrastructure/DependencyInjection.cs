using Microsoft.Extensions.DependencyInjection;

namespace QrFoodOrdering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Sprint 0 Day1 ยังไม่ผูก DB/External service
        return services;
    }
}
