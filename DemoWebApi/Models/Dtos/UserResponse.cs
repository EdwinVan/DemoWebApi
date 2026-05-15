using DemoWebApi.Models.Entities;

namespace DemoWebApi.Models.Dtos;

/// <summary>
/// 用户响应数据
/// </summary>
public class UserResponse
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    public static UserResponse FromEntity(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Age = user.Age,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };
    }
}
