namespace QrFoodOrdering.Application.Common.Observability;

public sealed class TraceContext : ITraceContext
{
    public string TraceId { get; set; } = "unknown";
}
