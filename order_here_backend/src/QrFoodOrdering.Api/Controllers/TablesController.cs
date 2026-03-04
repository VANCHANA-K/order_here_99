using Microsoft.AspNetCore.Mvc;
using QrFoodOrdering.Api.Contracts.Tables;
using QrFoodOrdering.Application.Qr.Generate;
using QrFoodOrdering.Application.Tables.Create;
using QrFoodOrdering.Application.Tables.GetAll;
using QrFoodOrdering.Application.Tables.UpdateStatus;

namespace QrFoodOrdering.Api.Controllers;

[ApiController]
[Route("api/v1/tables")]
public sealed class TablesController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<GetAllTablesResult>>> GetAll(
        [FromServices] GetAllTablesHandler handler,
        CancellationToken ct
    )
    {
        var result = await handler.Handle(ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTableRequest request,
        [FromServices] CreateTableHandler handler,
        CancellationToken ct
    )
    {
        var id = await handler.Handle(new CreateTableCommand(request.Code), ct);
        return Created($"/api/v1/tables/{id}", new { id });
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(
        Guid id,
        [FromServices] UpdateTableStatusHandler handler,
        CancellationToken ct
    )
    {
        await handler.Handle(new UpdateTableStatusCommand(id, true), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/disable")]
    public async Task<IActionResult> Disable(
        Guid id,
        [FromServices] UpdateTableStatusHandler handler,
        CancellationToken ct
    )
    {
        await handler.Handle(new UpdateTableStatusCommand(id, false), ct);
        return NoContent();
    }

    // ===============================
    // BE-35 Generate QR Code API
    // ===============================
    [HttpPost("{id:guid}/qr")]
    public async Task<IActionResult> GenerateQr(
        [FromRoute] Guid id,
        [FromServices] GenerateQrHandler handler,
        CancellationToken ct
    )
    {
        var result = await handler.HandleAsync(id, ct);
        return Ok(result);
    }
}
