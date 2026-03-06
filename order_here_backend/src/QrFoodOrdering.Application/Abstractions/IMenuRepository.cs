using QrFoodOrdering.Domain.Menu;

namespace QrFoodOrdering.Application.Abstractions;

public interface IMenuRepository
{
    Task<IReadOnlyList<MenuItem>> GetMenuForTableAsync(Guid tableId, CancellationToken ct);
    Task<List<MenuItem>> GetByIdsAsync(List<Guid> ids, CancellationToken ct);
}
