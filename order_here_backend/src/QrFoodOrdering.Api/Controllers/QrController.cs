using Microsoft.AspNetCore.Mvc;
using QrFoodOrdering.Api.Contracts.Common;
using QrFoodOrdering.Api.Contracts.Qr;
using QrFoodOrdering.Application.Qr.Resolve;

namespace QrFoodOrdering.Api.Controllers;

[ApiController]
[Route("api/v1/qr")]
[Produces("application/json")]
public sealed class QrController : ControllerBase
{
    private readonly ResolveQrHandler _handler;

    public QrController(ResolveQrHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("{token}")]
    [ProducesResponseType(typeof(ResolveQrResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResolveQrResponse>> Resolve(string token, CancellationToken ct)
    {
        var result = await _handler.HandleAsync(token, ct);
        return Ok(new ResolveQrResponse(result.TableId, result.TableCode));
    }
}
