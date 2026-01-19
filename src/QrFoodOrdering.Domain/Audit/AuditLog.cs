namespace QrFoodOrdering.Domain.Audit;

public sealed class AuditLog
{
    public DateTimeOffset Timestamp { get; }
    public string TraceId { get; }
    public string Action { get; }
    public string Detail { get; }

    public AuditLog(string traceId, string action, string detail)
    {
        TraceId = traceId;
        Action = action;
        Detail = detail;
        Timestamp = DateTimeOffset.UtcNow;
    }
}
