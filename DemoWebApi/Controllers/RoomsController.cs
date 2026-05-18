using DemoWebApi.Models.Dtos;
using DemoWebApi.Models.Entities;
using DemoWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [Authorize(Roles = nameof(UserRole.SuperAdmin))]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoomRequest request)
    {
        var result = await _roomService.CreateAsync(request);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(byte id)
    {
        var result = await _roomService.GetByIdAsync(id);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _roomService.GetAllAsync();
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id}/things")]
    public async Task<IActionResult> GetThings(byte id)
    {
        var result = await _roomService.GetThingsAsync(id);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }

    [Authorize(Roles = nameof(UserRole.SuperAdmin))]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(byte id, [FromBody] UpdateRoomRequest request)
    {
        var result = await _roomService.UpdateAsync(id, request);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }

    [Authorize(Roles = nameof(UserRole.SuperAdmin))]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(byte id)
    {
        var result = await _roomService.DeleteAsync(id);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }
}
