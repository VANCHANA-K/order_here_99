using System.Text.Json;
using QrFoodOrdering.Application.Common.Audit;
using QrFoodOrdering.Application.Common.Observability;
using QrFoodOrdering.Domain.Audit;

namespace QrFoodOrdering.Infrastructure.Audit;

public sealed class AuditLogger : IAuditLogger
{
    private readonly ITraceContext _trace;
    private readonly IAuditLogWriter _writer;

    public AuditLogger(ITraceContext trace, IAuditLogWriter writer)
    {
        _trace = trace;
        _writer = writer;
    }

    public Task LogAsync(string eventType, string entityType, Guid entityId, object data, CancellationToken ct)
    {
        var traceId = TraceIdPolicy.Resolve(_trace.TraceId, "audit-writer");
        var detail = JsonSerializer.Serialize(new { traceId, entityId, data });
        var log = new AuditLog(eventType, entityType, entityId, detail, traceId);
        return _writer.WriteAsync(log);
    }
}
