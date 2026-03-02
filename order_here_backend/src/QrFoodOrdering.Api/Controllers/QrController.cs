using Microsoft.AspNetCore.Mvc;
using QrFoodOrdering.Application.Qr.Resolve;

namespace QrFoodOrdering.Api.Controllers;

[ApiController]
[Route("api/v1/qr")]
public class QrController : ControllerBase
{
    private readonly ResolveQrHandler _handler;

    public QrController(ResolveQrHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> Resolve(string token, CancellationToken ct)
    {
        var result = await _handler.HandleAsync(token, ct);
        return Ok(result);
    }
}
