namespace QrFoodOrdering.Api.Contracts.Orders;

public sealed record CreateOrderRequest(
    Guid TableId
);
