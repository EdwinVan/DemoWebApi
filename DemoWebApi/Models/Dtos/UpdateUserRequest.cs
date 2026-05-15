namespace DemoWebApi.Models.Dtos;

/// <summary>
/// 更新用户请求参数
/// </summary>
public class UpdateUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsActive { get; set; }
}
