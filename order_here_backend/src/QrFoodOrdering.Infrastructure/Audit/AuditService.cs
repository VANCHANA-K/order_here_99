using QrFoodOrdering.Application.Common.Audit;
using QrFoodOrdering.Domain.Audit;
using QrFoodOrdering.Infrastructure.Persistence;

namespace QrFoodOrdering.Infrastructure.Audit;

public class AuditService : IAuditService
{
    private readonly QrFoodOrderingDbContext _db;

    public AuditService(QrFoodOrderingDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(
        string eventType,
        string entityType,
        Guid entityId,
        string? metadata = null)
    {
        var log = new AuditLog(
            eventType,
            entityType,
            entityId,
            metadata);

        _db.AuditLogs.Add(log);

        await _db.SaveChangesAsync();
    }
}
