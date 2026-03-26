using System.Collections.Concurrent;

namespace QrFoodOrdering.Api.Infrastructure;

public sealed class InFlightRequestGate : IInFlightRequestGate
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task<T> ExecuteAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> action,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key))
            return await action(ct);

        var gate = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            return await action(ct);
        }
        finally
        {
            gate.Release();
        }
    }
}
