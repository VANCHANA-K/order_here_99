namespace QrFoodOrdering.Application.Common.Audit;

public interface IAuditLogger
{
    Task LogAsync(string eventType, string entityType, Guid entityId, object data, CancellationToken ct);
}
