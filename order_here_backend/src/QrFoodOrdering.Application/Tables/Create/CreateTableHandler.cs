using QrFoodOrdering.Application.Common.Audit;
using QrFoodOrdering.Domain.Tables;
using QrFoodOrdering.Application.Abstractions;

namespace QrFoodOrdering.Application.Tables.Create;

public sealed class CreateTableHandler
{
    private readonly ITablesRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IAuditLogger _audit;

    public CreateTableHandler(ITablesRepository repo, IUnitOfWork uow, IAuditLogger audit)
    {
        _repo = repo;
        _uow = uow;
        _audit = audit;
    }

    public async Task<Guid> Handle(CreateTableCommand cmd, CancellationToken ct)
    {
        var table = new Table(cmd.Code);

        await _repo.AddAsync(table, ct);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(AuditEvents.TableCreated, AuditEntities.Table, table.Id, new { table.Code }, ct);

        return table.Id;
    }
}
