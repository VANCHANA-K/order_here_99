using QrFoodOrdering.Domain.Tables;

namespace QrFoodOrdering.Application.Tables;

public interface ITablesRepository
{
    Task AddAsync(Table table, CancellationToken ct);
    Task<Table?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<Table>> GetAllAsync(CancellationToken ct);
}
