using Dapper.Contrib.Extensions;
using SqlSugar;

namespace DemoWebApi.Models.Entities;

/// <summary>
/// 用户表
/// </summary>
[Table("users")]
public class User
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Key]
    [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [SugarColumn(ColumnName = "user_name")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    [SugarColumn(ColumnName = "email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 年龄
    /// </summary>
    [SugarColumn(ColumnName = "age")]
    public int Age { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnName = "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 是否启用
    /// </summary>
    [SugarColumn(ColumnName = "is_active")]
    public bool IsActive { get; set; } = true;
}
