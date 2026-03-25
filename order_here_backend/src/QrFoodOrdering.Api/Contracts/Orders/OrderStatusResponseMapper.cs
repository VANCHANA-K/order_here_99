using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Api.Contracts.Orders;

internal static class OrderStatusResponseMapper
{
    public static string ToResponseStatus(OrderStatus status) =>
        status switch
        {
            OrderStatus.Pending => "Pending",
            OrderStatus.Confirmed => "Confirmed",
            OrderStatus.Cancelled => "Cancelled",
            OrderStatus.Completed => "Completed",
            OrderStatus.Cooking => "Cooking",
            OrderStatus.Ready => "Ready",
            OrderStatus.Served => "Served",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported order status.")
        };
}
