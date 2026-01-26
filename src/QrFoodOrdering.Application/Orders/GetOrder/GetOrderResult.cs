namespace QrFoodOrdering.Application.Orders.GetOrder;

public sealed record GetOrderResult(
    Guid OrderId,
    string Status,
    decimal TotalAmount
);
