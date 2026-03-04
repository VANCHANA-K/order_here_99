using QrFoodOrdering.Application.Common.Audit;
using QrFoodOrdering.Application.Common.Exceptions;

namespace QrFoodOrdering.Application.Tables.UpdateStatus;

public sealed class UpdateTableStatusHandler
{
    private readonly ITablesRepository _repo;
    private readonly IAuditLogger _audit;

    public UpdateTableStatusHandler(ITablesRepository repo, IAuditLogger audit)
    {
        _repo = repo;
        _audit = audit;
    }

    public async Task Handle(UpdateTableStatusCommand cmd, CancellationToken ct)
    {
        var table =
            await _repo.GetByIdAsync(cmd.TableId, ct)
            ?? throw new NotFoundException("TABLE_NOT_FOUND");

        if (cmd.Activate)
        {
            table.Activate();
        }
        else
        {
            table.Deactivate();
        }

        await _repo.SaveChangesAsync(ct);

        await _audit.LogAsync("TableStatusChanged", "Table", table.Id, new { table.IsActive }, ct);
    }
}
