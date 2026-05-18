using DemoWebApi.Models.Dtos;
using DemoWebApi.Models.Entities;
using DemoWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = nameof(UserRole.SuperAdmin))]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var result = await _userService.CreateAsync(request);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _userService.GetAllAsync();
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var result = await _userService.UpdateAsync(id, request);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.DeleteAsync(id);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }
}
