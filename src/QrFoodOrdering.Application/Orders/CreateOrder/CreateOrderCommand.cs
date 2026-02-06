namespace QrFoodOrdering.Application.Orders.CreateOrder;

public sealed record CreateOrderCommand(
    CreateOrderRequest Request,
    string? IdempotencyKey
);
