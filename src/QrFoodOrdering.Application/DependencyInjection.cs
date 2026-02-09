using Microsoft.Extensions.DependencyInjection;
using QrFoodOrdering.Application.Orders.AddItem;
using QrFoodOrdering.Application.Orders.CloseOrder;
using QrFoodOrdering.Application.Orders.CreateOrder;
using QrFoodOrdering.Application.Orders.GetOrder;
using QrFoodOrdering.Application.Common.Observability;

namespace QrFoodOrdering.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register Use Case handlers
        services.AddScoped<CreateOrderHandler>();
        services.AddScoped<AddItemHandler>();
        services.AddScoped<GetOrderHandler>();
        services.AddScoped<CloseOrderHandler>();

        // Observability: Trace context (scoped per request)
        services.AddScoped<ITraceContext, TraceContext>();

        return services;
    }
}
