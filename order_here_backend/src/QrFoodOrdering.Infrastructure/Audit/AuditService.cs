using QrFoodOrdering.Application.Common.Audit;
using QrFoodOrdering.Application.Common.Observability;
using QrFoodOrdering.Domain.Audit;
using QrFoodOrdering.Infrastructure.Persistence;

namespace QrFoodOrdering.Infrastructure.Audit;

public class AuditService : IAuditService
{
    private readonly QrFoodOrderingDbContext _db;
    private readonly ITraceContext _trace;

    public AuditService(QrFoodOrderingDbContext db, ITraceContext trace)
    {
        _db = db;
        _trace = trace;
    }

    public Task LogAsync(
        string eventType,
        string entityType,
        Guid entityId,
        string? metadata = null)
    {
        var traceId = TraceIdPolicy.Resolve(_trace.TraceId, "audit-db");
        var log = new AuditLog(
            eventType,
            entityType,
            entityId,
            metadata,
            traceId);

        _db.AuditLogs.Add(log);
        return Task.CompletedTask;
    }
}
