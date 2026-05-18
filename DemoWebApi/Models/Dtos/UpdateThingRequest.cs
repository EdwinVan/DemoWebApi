namespace DemoWebApi.Models.Dtos;

public class UpdateThingRequest
{
    public string? Color { get; set; }
    public decimal? Price { get; set; }
    public int? Number { get; set; }
    public string? Description { get; set; }
}
