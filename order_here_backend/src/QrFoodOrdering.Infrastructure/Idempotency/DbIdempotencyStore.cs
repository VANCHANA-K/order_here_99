using Microsoft.EntityFrameworkCore;
using QrFoodOrdering.Application.Common.Idempotency;
using QrFoodOrdering.Infrastructure.Persistence;

namespace QrFoodOrdering.Infrastructure.Idempotency;

public sealed class DbIdempotencyStore : IIdempotencyStore
{
    private readonly QrFoodOrderingDbContext _db;

    public DbIdempotencyStore(QrFoodOrderingDbContext db)
    {
        _db = db;
    }

    public async Task<IdempotencyResult> TryGetAsync(string key, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key))
            return new IdempotencyResult(false, string.Empty, Guid.Empty);

        var record = await _db.IdempotencyRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == key, ct);

        return record is null
            ? new IdempotencyResult(false, string.Empty, Guid.Empty)
            : new IdempotencyResult(true, record.RequestHash, record.OrderId);
    }

    public Task MarkAsync(string key, string requestHash, Guid orderId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Task.CompletedTask;

        _db.IdempotencyRecords.Add(new IdempotencyRecord(key, requestHash, orderId));
        return Task.CompletedTask;
    }
}
