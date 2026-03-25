using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Audit;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;

namespace QrFoodOrdering.Application.Tables.UpdateStatus;

public sealed class UpdateTableStatusHandler
{
    private readonly ITablesRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IAuditLogger _audit;

    public UpdateTableStatusHandler(ITablesRepository repo, IUnitOfWork uow, IAuditLogger audit)
    {
        _repo = repo;
        _uow = uow;
        _audit = audit;
    }

    public async Task Handle(UpdateTableStatusCommand cmd, CancellationToken ct)
    {
        var table =
            await _repo.GetByIdAsync(cmd.TableId, ct)
            ?? throw new NotFoundException(
                ApplicationErrorCodes.TableNotFound,
                "Table not found."
            );

        if (cmd.Activate)
        {
            table.Activate();
        }
        else
        {
            table.Deactivate();
        }

        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(
            AuditEvents.TableStatusChanged,
            AuditEntities.Table,
            table.Id,
            new { table.IsActive },
            ct
        );
    }
}
