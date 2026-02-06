using Microsoft.EntityFrameworkCore.Storage;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Infrastructure.Persistence;

namespace QrFoodOrdering.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly QrFoodOrderingDbContext _db;
    private IDbContextTransaction? _tx;

    public UnitOfWork(QrFoodOrderingDbContext db)
    {
        _db = db;
    }

    public async Task<IAsyncDisposable> BeginTransactionAsync(CancellationToken ct)
    {
        _tx = await _db.Database.BeginTransactionAsync(ct);
        return _tx;
    }

    public async Task CommitAsync(CancellationToken ct)
    {
        if (_tx is not null)
        {
            await _tx.CommitAsync(ct);
        }
    }
}

