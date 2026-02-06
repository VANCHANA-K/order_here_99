using System;
using System.Threading;
using System.Threading.Tasks;

namespace QrFoodOrdering.Application.Common.Idempotency;

public interface IIdempotencyStore
{
    Task<(bool Found, Guid OrderId)> TryGetAsync(string key, CancellationToken ct);
    Task MarkAsync(string key, Guid orderId, CancellationToken ct);
}

