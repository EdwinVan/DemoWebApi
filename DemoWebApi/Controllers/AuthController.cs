using System.Security.Claims;
using DemoWebApi.Models.Dtos;
using DemoWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdRaw, out var userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("登录状态无效", 401));
        }

        var result = await _authService.GetCurrentUserAsync(userId);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }
}
