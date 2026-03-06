using Microsoft.EntityFrameworkCore;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Domain.Menu;
using QrFoodOrdering.Infrastructure.Persistence;

namespace QrFoodOrdering.Infrastructure.Repositories;

public sealed class MenuRepository : IMenuRepository
{
    private readonly QrFoodOrderingDbContext _db;

    public MenuRepository(QrFoodOrderingDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<MenuItem>> GetMenuForTableAsync(Guid tableId, CancellationToken ct)
    {
        // Day5: menu is global (restaurant-level)
        // We accept tableId for contract/future extension
        return await _db.MenuItems
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<List<MenuItem>> GetByIdsAsync(List<Guid> ids, CancellationToken ct)
    {
        return await _db.MenuItems
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(ct);
    }
}
