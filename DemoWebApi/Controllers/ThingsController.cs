using DemoWebApi.Models.Dtos;
using DemoWebApi.Models.Entities;
using DemoWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ThingsController : ControllerBase
{
    private readonly IThingService _thingService;

    public ThingsController(IThingService thingService)
    {
        _thingService = thingService;
    }

    [Authorize(Roles = nameof(UserRole.SuperAdmin))]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateThingRequest request)
    {
        var result = await _thingService.CreateAsync(request);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _thingService.GetByIdAsync(id);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _thingService.GetAllAsync();
        return Ok(result);
    }

    [Authorize(Roles = nameof(UserRole.SuperAdmin))]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateThingRequest request)
    {
        var result = await _thingService.UpdateAsync(id, request);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }

    [Authorize(Roles = nameof(UserRole.SuperAdmin))]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _thingService.DeleteAsync(id);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }
}
