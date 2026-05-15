using DemoWebApi.Models.Entities;

namespace DemoWebApi.Models.Dtos;

public class ThingResponse
{
    public int Id { get; set; }
    public string? Color { get; set; }
    public decimal? Price { get; set; }
    public int? Number { get; set; }
    public string? Description { get; set; }

    public static ThingResponse FromEntity(Thing thing)
    {
        return new ThingResponse
        {
            Id = thing.Id,
            Color = thing.Color,
            Price = thing.Price,
            Number = thing.Number,
            Description = thing.Description
        };
    }
}
