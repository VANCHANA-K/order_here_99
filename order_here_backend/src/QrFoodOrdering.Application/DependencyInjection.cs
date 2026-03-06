using Microsoft.Extensions.DependencyInjection;
using QrFoodOrdering.Application.Common.Observability;
using QrFoodOrdering.Application.Common.Resilience;
using QrFoodOrdering.Application.Orders.AddItem;
using QrFoodOrdering.Application.Orders.CloseOrder;
using QrFoodOrdering.Application.Orders.CreateOrder;
using QrFoodOrdering.Application.Orders.CreateOrderViaQr;
using QrFoodOrdering.Application.Orders.GetOrder;
using QrFoodOrdering.Application.Menu.GetByTable;
using QrFoodOrdering.Application.Qr.Generate;
using QrFoodOrdering.Application.Tables.Create;
using QrFoodOrdering.Application.Tables.GetAll;
using QrFoodOrdering.Application.Tables.UpdateStatus;

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
        services.AddScoped<CreateTableHandler>();
        services.AddScoped<GetAllTablesHandler>();
        services.AddScoped<UpdateTableStatusHandler>();
        services.AddScoped<GenerateQrHandler>();
        services.AddScoped<CreateOrderViaQrHandler>();
        services.AddScoped<GetMenuByTableHandler>();

        // Observability: Trace context (scoped per request)
        services.AddScoped<ITraceContext, TraceContext>();

        // Resilience: simple retry policy (scoped per request)
        services.AddScoped<IRetryPolicy, SimpleRetryPolicy>();

        return services;
    }
}
