using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Application.Common.Validation;

namespace QrFoodOrdering.Api.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("test")]
public sealed class TestErrorController : ControllerBase
{
    private readonly IHostEnvironment _environment;

    public TestErrorController(IHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet("error")]
    public IActionResult Throw()
    {
        if (!_environment.IsEnvironment("Test"))
            return NotFound();

        throw new Exception("THIS_MESSAGE_MUST_NOT_LEAK");
    }

    [HttpGet("conflict/concurrency")]
    public IActionResult ThrowConcurrencyConflict()
    {
        if (!_environment.IsEnvironment("Test"))
            return NotFound();

        throw new ConflictException(
            ApplicationErrorCodes.ConcurrencyConflict,
            RequestValidationMessages.ConcurrencyConflict
        );
    }

    [HttpGet("configuration-invalid")]
    public IActionResult ThrowConfigurationInvalid()
    {
        if (!_environment.IsEnvironment("Test"))
            return NotFound();

        throw new ConfigurationValidationException(
            ApplicationErrorCodes.ConfigurationInvalid,
            "INTERNAL_CONFIGURATION_PATH_MUST_NOT_LEAK"
        );
    }

    [HttpGet("audit-log-immutable")]
    public IActionResult ThrowAuditLogImmutable()
    {
        if (!_environment.IsEnvironment("Test"))
            return NotFound();

        throw new ImmutableResourceException(
            ApplicationErrorCodes.AuditLogImmutable,
            "INTERNAL_AUDIT_IMMUTABILITY_DETAIL_MUST_NOT_LEAK"
        );
    }

    [HttpGet("timeout")]
    public IActionResult ThrowTimeout()
    {
        if (!_environment.IsEnvironment("Test"))
            return NotFound();

        throw new TimeoutException("INTERNAL_TIMEOUT_DETAIL_MUST_NOT_LEAK");
    }

    [HttpGet("database-unavailable")]
    public IActionResult ThrowDatabaseUnavailable()
    {
        if (!_environment.IsEnvironment("Test"))
            return NotFound();

        throw new ServiceUnavailableException(
            ApplicationErrorCodes.DatabaseUnavailable,
            "INTERNAL_DATABASE_DETAIL_MUST_NOT_LEAK"
        );
    }
}
