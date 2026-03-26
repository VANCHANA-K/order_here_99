using System.Collections.Concurrent;
using QrFoodOrdering.Application.Common.Idempotency;

namespace QrFoodOrdering.Infrastructure.Idempotency;

public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, IdempotencyResult> _map = new();

    public Task<IdempotencyResult> TryGetAsync(string key, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Task.FromResult(new IdempotencyResult(false, string.Empty, Guid.Empty));

        var found = _map.TryGetValue(key, out var value);
        return Task.FromResult(found ? value : new IdempotencyResult(false, string.Empty, Guid.Empty));
    }

    public Task MarkAsync(string key, string requestHash, Guid orderId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key)) return Task.CompletedTask;
        _map[key] = new IdempotencyResult(true, requestHash, orderId);
        return Task.CompletedTask;
    }
}
