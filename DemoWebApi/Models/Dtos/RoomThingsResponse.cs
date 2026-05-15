namespace DemoWebApi.Models.Dtos;

public class RoomThingsResponse
{
    public RoomResponse Room { get; set; } = new();
    public List<ThingResponse> Things { get; set; } = new();
}
