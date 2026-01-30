namespace QrFoodOrdering.Api.Contracts.Orders;

public sealed record OrderResponse(
    Guid OrderId,
    string Status,
    decimal TotalAmount
);
