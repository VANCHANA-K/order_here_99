using QrFoodOrdering.Domain.Orders;

namespace QrFoodOrdering.Application.Orders.GetOrder;

public sealed record GetOrderResult(
    Guid OrderId,
    OrderStatus Status,
    decimal TotalAmount
);
