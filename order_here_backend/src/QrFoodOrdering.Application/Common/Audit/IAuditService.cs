namespace QrFoodOrdering.Application.Common.Audit;

public interface IAuditService
{
    Task LogAsync(string eventType, string entityType, Guid entityId, string? metadata = null);
}
