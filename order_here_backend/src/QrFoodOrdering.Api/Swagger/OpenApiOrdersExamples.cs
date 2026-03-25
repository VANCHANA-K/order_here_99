using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Any;

namespace QrFoodOrdering.Api.Swagger;

internal sealed class OpenApiOrdersExamples : IOpenApiExampleCatalog
{
    public IReadOnlyDictionary<OpenApiExampleKey, IOpenApiAny> Examples =>
        new Dictionary<OpenApiExampleKey, IOpenApiAny>
        {
            [OpenApiExampleKey.Post(OpenApiRoutePaths.Orders, StatusCodes.Status201Created)] =
                OpenApiExampleRegistry.Object(("orderId", "11111111-1111-1111-1111-111111111111")),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.Orders, StatusCodes.Status400BadRequest)] =
                OpenApiExampleRegistry.Error("TABLE_ID_REQUIRED", "TableId is required."),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.Orders, StatusCodes.Status404NotFound)] =
                OpenApiExampleRegistry.Error("TABLE_NOT_FOUND", "Table not found."),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.Orders, StatusCodes.Status409Conflict)] =
                OpenApiExampleRegistry.Error("TABLE_INACTIVE", "Table is inactive."),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.OrderItems, StatusCodes.Status204NoContent)] =
                OpenApiExampleRegistry.Object(),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.OrderItems, StatusCodes.Status400BadRequest)] =
                OpenApiExampleRegistry.Error("PRODUCT_NAME_REQUIRED", "ProductName is required."),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.OrderItems, StatusCodes.Status404NotFound)] =
                OpenApiExampleRegistry.Error("ORDER_NOT_FOUND", "Order not found"),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.OrderItems, StatusCodes.Status409Conflict)] =
                OpenApiExampleRegistry.Error("ORDER_NOT_OPEN", "Order is not open."),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.OrdersViaQr, StatusCodes.Status200OK)] = OpenApiExampleRegistry.Object(
                ("orderId", "22222222-2222-2222-2222-222222222222"),
                ("status", "Pending"),
                ("createdAtUtc", "2026-03-25T12:00:00Z")
            ),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.OrdersViaQr, StatusCodes.Status400BadRequest)] =
                OpenApiExampleRegistry.Error("EMPTY_ITEMS", "At least one item is required."),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.OrdersViaQr, StatusCodes.Status404NotFound)] =
                OpenApiExampleRegistry.Error("TABLE_NOT_FOUND", "Table not found."),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.OrdersViaQr, StatusCodes.Status409Conflict)] =
                OpenApiExampleRegistry.Error("TABLE_INACTIVE", "Table is inactive."),
            [OpenApiExampleKey.Get(OpenApiRoutePaths.OrderById, StatusCodes.Status200OK)] = OpenApiExampleRegistry.Object(
                ("orderId", "33333333-3333-3333-3333-333333333333"),
                ("status", "Pending"),
                ("totalAmount", 120m)
            ),
            [OpenApiExampleKey.Get(OpenApiRoutePaths.OrderById, StatusCodes.Status404NotFound)] =
                OpenApiExampleRegistry.Error("ORDER_NOT_FOUND", "Order not found"),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.OrderClose, StatusCodes.Status204NoContent)] =
                OpenApiExampleRegistry.Object(),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.OrderClose, StatusCodes.Status404NotFound)] =
                OpenApiExampleRegistry.Error("ORDER_NOT_FOUND", "Order not found"),
            [OpenApiExampleKey.Post(OpenApiRoutePaths.OrderClose, StatusCodes.Status409Conflict)] =
                OpenApiExampleRegistry.Error("ORDER_NOT_OPEN", "Order is not open."),
        };
}
