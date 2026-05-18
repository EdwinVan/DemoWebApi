using Dapper.Contrib.Extensions;
using SqlSugar;

namespace DemoWebApi.Models.Entities;

[Table("users")]
public class User
{
    [Key]
    [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(ColumnName = "user_name")]
    public string UserName { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "email")]
    public string Email { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "age")]
    public int Age { get; set; }

    [SugarColumn(ColumnName = "password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "role")]
    public UserRole Role { get; set; } = UserRole.NormalUser;

    [SugarColumn(ColumnName = "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [SugarColumn(ColumnName = "is_active")]
    public bool IsActive { get; set; } = true;

    [SugarColumn(ColumnName = "last_login_at")]
    public DateTime? LastLoginAt { get; set; }
}
