namespace QrFoodOrdering.Domain.Audit;

public class AuditLog
{
    public Guid Id { get; private set; }

    public string EventType { get; private set; } = default!;

    public string EntityType { get; private set; } = default!;

    public Guid EntityId { get; private set; }

    public string? Metadata { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private AuditLog() { }

    public AuditLog(string eventType, string entityType, Guid entityId, string? metadata = null)
    {
        Id = Guid.NewGuid();
        EventType = eventType;
        EntityType = entityType;
        EntityId = entityId;
        Metadata = metadata;
        CreatedAtUtc = DateTime.UtcNow;
    }
}
