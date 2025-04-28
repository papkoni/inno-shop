using Microsoft.AspNetCore.Mvc;

namespace ProductService.Presentation.Controllers;

[ApiController]
public class ProductController: ControllerBase
{
    [HttpGet("mock")]
    public async Task<IActionResult> Mock()
    {
        return Ok();
    }

}