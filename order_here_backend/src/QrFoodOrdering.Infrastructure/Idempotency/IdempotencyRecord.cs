namespace QrFoodOrdering.Infrastructure.Idempotency;

public sealed class IdempotencyRecord
{
    public string Key { get; private set; } = string.Empty;
    public string RequestHash { get; private set; } = string.Empty;
    public Guid OrderId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private IdempotencyRecord() { }

    public IdempotencyRecord(string key, string requestHash, Guid orderId)
    {
        Key = key;
        RequestHash = requestHash;
        OrderId = orderId;
        CreatedAtUtc = DateTime.UtcNow;
    }
}
