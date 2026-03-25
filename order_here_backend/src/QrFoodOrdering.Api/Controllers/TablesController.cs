using Microsoft.AspNetCore.Mvc;
using QrFoodOrdering.Api.Contracts.Common;
using QrFoodOrdering.Api.Contracts.Qr;
using QrFoodOrdering.Api.Contracts.Tables;
using QrFoodOrdering.Application.Qr.Generate;
using QrFoodOrdering.Application.Tables.Create;
using QrFoodOrdering.Application.Tables.GetAll;
using QrFoodOrdering.Application.Tables.UpdateStatus;

namespace QrFoodOrdering.Api.Controllers;

[ApiController]
[Route("api/v1/tables")]
[Produces("application/json")]
public sealed class TablesController : ControllerBase
{
    private readonly GetAllTablesHandler _getAllTablesHandler;
    private readonly CreateTableHandler _createTableHandler;
    private readonly UpdateTableStatusHandler _updateTableStatusHandler;
    private readonly GenerateQrHandler _generateQrHandler;

    public TablesController(
        GetAllTablesHandler getAllTablesHandler,
        CreateTableHandler createTableHandler,
        UpdateTableStatusHandler updateTableStatusHandler,
        GenerateQrHandler generateQrHandler
    )
    {
        _getAllTablesHandler = getAllTablesHandler;
        _createTableHandler = createTableHandler;
        _updateTableStatusHandler = updateTableStatusHandler;
        _generateQrHandler = generateQrHandler;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<TableResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<TableResponse>>> GetAll(CancellationToken ct)
    {
        var result = await _getAllTablesHandler.Handle(ct);
        var response = result
            .Select(x => new TableResponse(x.Id, x.Code, x.Status))
            .ToList();
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreatedIdResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreatedIdResponse>> Create(
        [FromBody] CreateTableRequest request,
        CancellationToken ct
    )
    {
        var id = await _createTableHandler.Handle(new CreateTableCommand(request.Code), ct);
        return Created($"/api/v1/tables/{id}", new CreatedIdResponse(id));
    }

    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _updateTableStatusHandler.Handle(new UpdateTableStatusCommand(id, true), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/disable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Disable(Guid id, CancellationToken ct)
    {
        await _updateTableStatusHandler.Handle(new UpdateTableStatusCommand(id, false), ct);
        return NoContent();
    }

    // ===============================
    // BE-35 Generate QR Code API
    // ===============================
    [HttpPost("{id:guid}/qr")]
    [ProducesResponseType(typeof(GenerateQrResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GenerateQrResponse>> GenerateQr([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _generateQrHandler.HandleAsync(id, ct);
        return Ok(new GenerateQrResponse(
            result.TableId,
            result.Token,
            result.QrUrl,
            result.GeneratedAtUtc
        ));
    }
}
