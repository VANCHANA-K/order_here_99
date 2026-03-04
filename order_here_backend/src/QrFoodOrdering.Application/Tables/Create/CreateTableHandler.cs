using QrFoodOrdering.Application.Common.Audit;
using QrFoodOrdering.Domain.Tables;

namespace QrFoodOrdering.Application.Tables.Create;

public sealed class CreateTableHandler
{
    private readonly ITablesRepository _repo;
    private readonly IAuditLogger _audit;

    public CreateTableHandler(ITablesRepository repo, IAuditLogger audit)
    {
        _repo = repo;
        _audit = audit;
    }

    public async Task<Guid> Handle(CreateTableCommand cmd, CancellationToken ct)
    {
        var table = new Table(cmd.Code);

        await _repo.AddAsync(table, ct);
        await _repo.SaveChangesAsync(ct);

        await _audit.LogAsync("TableCreated", "Table", table.Id, new { table.Code }, ct);

        return table.Id;
    }
}
