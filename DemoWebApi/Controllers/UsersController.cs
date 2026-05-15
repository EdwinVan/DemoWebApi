using DemoWebApi.Models.Dtos;
using DemoWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DemoWebApi.Controllers;

/// <summary>
/// 用户管理 API
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService userService;

    public UsersController(IUserService userService)
    {
        this.userService = userService;
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var result = await userService.CreateAsync(request);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }

    /// <summary>
    /// 根据ID查询用户
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await userService.GetByIdAsync(id);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }

    /// <summary>
    /// 查询所有用户
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await userService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var result = await userService.UpdateAsync(id, request);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await userService.DeleteAsync(id);
        return StatusCode(result.Code == 200 ? 200 : result.Code, result);
    }
}
