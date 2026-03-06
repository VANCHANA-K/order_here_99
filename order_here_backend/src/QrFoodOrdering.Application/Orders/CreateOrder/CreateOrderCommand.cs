namespace QrFoodOrdering.Application.Orders.CreateOrder;

public sealed record CreateOrderCommand(
    Guid TableId,
    string? IdempotencyKey
);
