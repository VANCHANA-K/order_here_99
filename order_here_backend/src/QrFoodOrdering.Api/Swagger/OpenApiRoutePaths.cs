namespace QrFoodOrdering.Api.Swagger;

internal static class OpenApiRoutePaths
{
    public const string Health = "/health";

    public const string Orders = "/api/v1/orders";
    public const string OrderById = "/api/v1/orders/{id}";
    public const string OrderItems = "/api/v1/orders/{id}/items";
    public const string OrderClose = "/api/v1/orders/{id}/close";
    public const string OrdersViaQr = "/api/v1/orders/qr";

    public const string Tables = "/api/v1/tables";
    public const string TableActivate = "/api/v1/tables/{id}/activate";
    public const string TableDisable = "/api/v1/tables/{id}/disable";
    public const string TableQr = "/api/v1/tables/{id}/qr";
    public const string TableMenu = "/api/v1/tables/{tableId}/menu";

    public const string QrResolve = "/api/v1/qr/{token}";
}
