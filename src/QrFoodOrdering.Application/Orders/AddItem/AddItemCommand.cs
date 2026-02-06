public sealed record AddItemCommand(
    Guid OrderId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string? IdempotencyKey
);