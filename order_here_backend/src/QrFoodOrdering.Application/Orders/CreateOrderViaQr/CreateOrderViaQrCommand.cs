namespace QrFoodOrdering.Application.Orders.CreateOrderViaQr;

public sealed record CreateOrderViaQrCommand(
    Guid TableId,
    IReadOnlyList<CreateOrderViaQrItem> Items,
    string? IdempotencyKey
);

public sealed record CreateOrderViaQrItem(Guid MenuItemId, int Quantity);
