using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using QrFoodOrdering.Domain.Audit;
using QrFoodOrdering.Infrastructure.Audit;

namespace QrFoodOrdering.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    private readonly IAuditLogWriter _audit;

    public HealthController(IAuditLogWriter audit)
    {
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var traceId = HttpContext.TraceIdentifier;

        await _audit.WriteAsync(
            new AuditLog(
                "HEALTH_CHECK",
                "Health",
                Guid.Empty,
                JsonSerializer.Serialize(new { traceId, message = "Health endpoint accessed" })
            )
        );

        return Ok(new
        {
            status = "ok"
        });

    }
}
