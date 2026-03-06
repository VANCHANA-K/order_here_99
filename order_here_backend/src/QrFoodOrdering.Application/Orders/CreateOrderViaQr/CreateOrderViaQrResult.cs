using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Application.Orders.CreateOrderViaQr;

public sealed record CreateOrderViaQrResult(
    Guid OrderId,
    OrderStatus Status,
    DateTime CreatedAtUtc
);
