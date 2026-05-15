using Dapper.Contrib.Extensions;
using SqlSugar;

namespace DemoWebApi.Models.Entities;

/// <summary>
/// 房间表
/// </summary>
[Table("room")]
public class Room
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Key]
    [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsIdentity = true)]
    public byte Id { get; set; }

    /// <summary>
    /// 电脑Id
    /// </summary>
    [SugarColumn(ColumnName = "computer_id")]
    public int? ComputerId { get; set; }

    /// <summary>
    /// 床Id
    /// </summary>
    [SugarColumn(ColumnName = "bed_id")]
    public int? BedId { get; set; }
}
