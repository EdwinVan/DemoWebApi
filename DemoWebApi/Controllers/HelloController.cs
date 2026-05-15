using Microsoft.AspNetCore.Mvc;

namespace DemoWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    /// <summary>
    /// 简单问候
    /// </summary>
    [HttpGet("hello")]
    public IActionResult Health()
    {
        return Ok(new
        {
            Hello = "Hello World！"
        });
    }
}