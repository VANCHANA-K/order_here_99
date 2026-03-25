using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace QrFoodOrdering.Api.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("test/error")]
public sealed class TestErrorController : ControllerBase
{
    private readonly IHostEnvironment _environment;

    public TestErrorController(IHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Throw()
    {
        if (!_environment.IsEnvironment("Test"))
            return NotFound();

        throw new Exception("THIS_MESSAGE_MUST_NOT_LEAK");
    }
}
