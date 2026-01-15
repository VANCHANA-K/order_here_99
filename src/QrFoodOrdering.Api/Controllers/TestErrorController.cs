using Microsoft.AspNetCore.Mvc;

namespace QrFoodOrdering.Api.Controllers;

[ApiController]
[Route("test/error")]
public sealed class TestErrorController : ControllerBase
{
    [HttpGet]
    public IActionResult Throw()
    {
        throw new Exception("THIS_MESSAGE_MUST_NOT_LEAK");
    }
}
