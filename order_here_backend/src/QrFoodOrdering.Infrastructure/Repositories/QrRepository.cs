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
}
