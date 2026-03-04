using Microsoft.EntityFrameworkCore;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Domain.Qr;
using QrFoodOrdering.Infrastructure.Persistence;

namespace QrFoodOrdering.Infrastructure.Repositories;

public sealed class QrRepository : IQrRepository
{
    private readonly QrFoodOrderingDbContext _db;

    public QrRepository(QrFoodOrderingDbContext db)
    {
        _db = db;
    }

    public Task<QrCode?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return _db.QrCodes.AsNoTracking().FirstOrDefaultAsync(x => x.Token == token, ct);
    }

    public Task AddAsync(QrCode qrCode, CancellationToken ct)
    {
        return _db.QrCodes.AddAsync(qrCode, ct).AsTask();
    }

    public Task<List<QrCode>> GetActiveByTableIdAsync(
        Guid tableId,
        CancellationToken ct = default
    )
    {
        return _db
            .QrCodes
            .Where(x => x.TableId == tableId && x.IsActive)
            .ToListAsync(ct);
    }
}
