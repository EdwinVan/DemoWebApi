using DemoWebApi.Models.Dtos;
using DemoWebApi.Services;
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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateThingRequest request)
    {
        var result = await _thingService.CreateAsync(request);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _thingService.GetByIdAsync(id);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _thingService.GetAllAsync();
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateThingRequest request)
    {
        var result = await _thingService.UpdateAsync(id, request);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _thingService.DeleteAsync(id);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }
}
