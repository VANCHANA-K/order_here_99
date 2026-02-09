namespace QrFoodOrdering.Application.Common.Observability;

public interface ITraceContext
{
    string TraceId { get; }
}
