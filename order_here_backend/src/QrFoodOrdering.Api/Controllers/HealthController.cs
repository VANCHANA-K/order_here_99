using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using QrFoodOrdering.Api.Contracts.Common;
using QrFoodOrdering.Api.Middleware;
using QrFoodOrdering.Infrastructure.Persistence;

namespace QrFoodOrdering.Api.Controllers;

[ApiController]
[Route("health")]
[Produces("application/json")]
public sealed class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthChecks;

    public HealthController(HealthCheckService healthChecks)
    {
        _healthChecks = healthChecks;
    }

    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public ActionResult<HealthResponse> Get()
    {
        return Ok(new HealthResponse("ok"));
    }

    [HttpGet("live")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public ActionResult<HealthResponse> Live()
    {
        return Ok(new HealthResponse("ok"));
    }

    [HttpGet("ready")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<HealthResponse>> Ready(CancellationToken ct)
    {
        var report = await _healthChecks.CheckHealthAsync(
            registration => registration.Tags.Contains("ready"),
            ct
        );

        if (report.Status == HealthStatus.Healthy)
            return Ok(new HealthResponse("ok"));

        var traceId = Response.Headers[TraceIdMiddleware.HeaderName].ToString();
        if (string.IsNullOrWhiteSpace(traceId))
            traceId = HttpContext.TraceIdentifier;

        return StatusCode(
            StatusCodes.Status503ServiceUnavailable,
            new ApiErrorResponse(
                ApiErrorCodes.ServiceUnavailable,
                "Service is not ready.",
                traceId
            )
        );
    }
}
