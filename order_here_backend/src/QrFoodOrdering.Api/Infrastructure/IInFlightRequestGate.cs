namespace QrFoodOrdering.Api.Infrastructure;

public interface IInFlightRequestGate
{
    Task<T> ExecuteAsync<T>(string key, Func<CancellationToken, Task<T>> action, CancellationToken ct);
}
