using System.Collections.Concurrent;
using QrFoodOrdering.Application.Common.Idempotency;

namespace QrFoodOrdering.Infrastructure.Idempotency;

public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, Guid> _map = new();

    public Task<(bool Found, Guid OrderId)> TryGetAsync(string key, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Task.FromResult((false, Guid.Empty));

        var found = _map.TryGetValue(key, out var id);
        return Task.FromResult((found, id));
    }

    public Task MarkAsync(string key, Guid orderId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key)) return Task.CompletedTask;
        _map[key] = orderId;
        return Task.CompletedTask;
    }
}

