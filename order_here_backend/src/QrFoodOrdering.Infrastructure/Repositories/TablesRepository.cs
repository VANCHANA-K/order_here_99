using Microsoft.EntityFrameworkCore;
using QrFoodOrdering.Application.Tables;
using QrFoodOrdering.Domain.Tables;
using QrFoodOrdering.Infrastructure.Persistence;

namespace QrFoodOrdering.Infrastructure.Repositories;

public sealed class TablesRepository : ITablesRepository
{
    private readonly QrFoodOrderingDbContext _db;

    public TablesRepository(QrFoodOrderingDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(Table table, CancellationToken ct) =>
        _db.Tables.AddAsync(table, ct).AsTask();

    public Task<Table?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Tables.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<Table>> GetAllAsync(CancellationToken ct) =>
        _db.Tables.AsNoTracking().ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
