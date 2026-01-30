namespace QrFoodOrdering.Api.Contracts.Orders;

public sealed record AddItemRequest(
    string ProductName,
    int Quantity,
    decimal UnitPrice
);
