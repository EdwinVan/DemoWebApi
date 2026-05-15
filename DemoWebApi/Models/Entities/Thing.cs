using Dapper.Contrib.Extensions;
using SqlSugar;

namespace DemoWebApi.Models.Entities;

/// <summary>
/// 物品表
/// </summary>
[Table("thing")]
public class Thing
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Key]
    [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsIdentity = false)]
    public int Id { get; set; }

    /// <summary>
    /// 颜色
    /// </summary>
    [SugarColumn(ColumnName = "color")]
    public string? Color { get; set; }

    /// <summary>
    /// 价格
    /// </summary>
    [SugarColumn(ColumnName = "price")]
    public decimal? Price { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    [SugarColumn(ColumnName = "number")]
    public int? Number { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(ColumnName = "description")]
    public string? Description { get; set; }
}
