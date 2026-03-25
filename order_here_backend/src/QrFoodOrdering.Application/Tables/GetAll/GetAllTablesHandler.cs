using QrFoodOrdering.Application.Tables;
using QrFoodOrdering.Domain.Tables;

namespace QrFoodOrdering.Application.Tables.GetAll;

public sealed class GetAllTablesHandler
{
    private readonly ITablesRepository _repository;

    public GetAllTablesHandler(ITablesRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<GetAllTablesResult>> Handle(CancellationToken ct)
    {
        var tables = await _repository.GetAllAsync(ct);

        return tables
            .Select(t => new GetAllTablesResult(
                t.Id,
                t.Code,
                t.IsActive ? "Active" : "Inactive"
            ))
            .ToList();
    }
}
