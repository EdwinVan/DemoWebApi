using DemoWebApi.Models.Entities;

namespace DemoWebApi.Models.Dtos;

public class RoomResponse
{
    public byte Id { get; set; }
    public int? ComputerId { get; set; }
    public int? BedId { get; set; }

    public static RoomResponse FromEntity(Room room)
    {
        return new RoomResponse
        {
            Id = room.Id,
            ComputerId = room.ComputerId,
            BedId = room.BedId
        };
    }
}
