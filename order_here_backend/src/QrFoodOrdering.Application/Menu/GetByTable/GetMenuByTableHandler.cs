using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Application.Tables;
using QrFoodOrdering.Application.Common.Validation;

namespace QrFoodOrdering.Application.Menu.GetByTable;

public sealed class GetMenuByTableHandler
{
    private readonly IMenuRepository _menuRepository;
    private readonly ITablesRepository _tablesRepository;

    public GetMenuByTableHandler(IMenuRepository menuRepository, ITablesRepository tablesRepository)
    {
        _menuRepository = menuRepository;
        _tablesRepository = tablesRepository;
    }

    public async Task<IReadOnlyList<GetMenuByTableResult>> Handle(
        GetMenuByTableQuery query,
        CancellationToken ct
    )
    {
        if (query.TableId == Guid.Empty)
            throw new InvalidRequestException(
                ApplicationErrorCodes.TableIdRequired,
                RequestValidationMessages.TableIdRequired
            );

        var table = await _tablesRepository.GetByIdAsync(query.TableId, ct);
        if (table is null)
            throw new NotFoundException(
                ApplicationErrorCodes.TableNotFound,
                "Table not found."
            );

        var items = await _menuRepository.GetMenuForTableAsync(query.TableId, ct);

        // BE-40: hide inactive items
        return items
            .Where(x => x.IsActive)
            .Select(x => new GetMenuByTableResult(x.Id, x.Code, x.Name, x.Price, x.IsAvailable))
            .ToList();
    }
}
