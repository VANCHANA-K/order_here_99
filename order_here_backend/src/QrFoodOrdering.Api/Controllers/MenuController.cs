using Microsoft.AspNetCore.Mvc;
using QrFoodOrdering.Api.Contracts.Common;
using QrFoodOrdering.Api.Contracts.Menu;
using QrFoodOrdering.Application.Menu.GetByTable;

namespace QrFoodOrdering.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public sealed class MenuController : ControllerBase
{
    private readonly GetMenuByTableHandler _handler;

    public MenuController(GetMenuByTableHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("tables/{tableId:guid}/menu")]
    [ProducesResponseType(typeof(IReadOnlyList<MenuItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<MenuItemResponse>>> GetMenuByTable(
        [FromRoute] Guid tableId,
        CancellationToken ct)
    {
        var result = await _handler.Handle(new GetMenuByTableQuery(tableId), ct);

        var response = result
            .Select(x => new MenuItemResponse(x.Id, x.Code, x.Name, x.Price, x.IsAvailable))
            .ToList();

        return Ok(response);
    }
}
