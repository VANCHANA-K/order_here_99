namespace QrFoodOrdering.Api.Contracts.Orders;

public sealed record CreateOrderViaQrRequest(
    Guid TableId,
    List<CreateOrderViaQrItemRequest> Items,
    string? IdempotencyKey
);

public sealed record CreateOrderViaQrItemRequest(
    Guid MenuItemId,
    int Quantity
);
