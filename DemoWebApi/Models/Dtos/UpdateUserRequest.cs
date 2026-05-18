using DemoWebApi.Models.Entities;

namespace DemoWebApi.Models.Dtos;

public class UpdateUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public UserRole Role { get; set; } = UserRole.NormalUser;
    public string? NewPassword { get; set; }
}
