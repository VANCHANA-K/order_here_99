using System;
using System.Threading;
using System.Threading.Tasks;

namespace QrFoodOrdering.Application.Common.Idempotency;

public interface IIdempotencyStore
{
    Task<IdempotencyResult> TryGetAsync(string key, CancellationToken ct);
    Task MarkAsync(string key, string requestHash, Guid orderId, CancellationToken ct);
}
