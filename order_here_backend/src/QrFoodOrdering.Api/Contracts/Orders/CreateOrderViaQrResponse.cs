namespace QrFoodOrdering.Api.Contracts.Orders;

public sealed record CreateOrderViaQrResponse(
    Guid OrderId,
    string Status,
    DateTime CreatedAtUtc
);
