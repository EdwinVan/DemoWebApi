using DemoWebApi.Data;
using DemoWebApi.Models.Dtos;
using DemoWebApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemoWebApi.Services;

public class ThingService : IThingService
{
    private static readonly HashSet<string> AllowedColors = new(StringComparer.OrdinalIgnoreCase)
    {
        "Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet"
    };

    private readonly AppDbContext _dbContext;

    public ThingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<ThingResponse>> CreateAsync(CreateThingRequest request)
    {
        if (!IsValidColor(request.Color))
        {
            return ApiResponse<ThingResponse>.Fail("Color must be one of the 7 allowed values.", 400);
        }

        var maxId = await _dbContext.Things.Select(t => (int?)t.Id).MaxAsync() ?? 0;
        var thing = new Thing
        {
            Id = maxId + 1,
            Color = request.Color,
            Price = RoundPrice(request.Price),
            Number = request.Number,
            Description = request.Description
        };

        _dbContext.Things.Add(thing);
        await _dbContext.SaveChangesAsync();

        return ApiResponse<ThingResponse>.Success(ThingResponse.FromEntity(thing), "Created.");
    }

    public async Task<ApiResponse<ThingResponse?>> GetByIdAsync(int id)
    {
        var thing = await _dbContext.Things.FindAsync(id);
        if (thing == null)
        {
            return ApiResponse<ThingResponse?>.Fail("Thing not found.", 404);
        }

        return ApiResponse<ThingResponse?>.Success(ThingResponse.FromEntity(thing));
    }

    public async Task<ApiResponse<List<ThingResponse>>> GetAllAsync()
    {
        var things = await _dbContext.Things
            .OrderBy(t => t.Id)
            .ToListAsync();

        return ApiResponse<List<ThingResponse>>.Success(things.Select(ThingResponse.FromEntity).ToList());
    }

    public async Task<ApiResponse<ThingResponse>> UpdateAsync(int id, UpdateThingRequest request)
    {
        var thing = await _dbContext.Things.FindAsync(id);
        if (thing == null)
        {
            return ApiResponse<ThingResponse>.Fail("Thing not found.", 404);
        }

        if (!IsValidColor(request.Color))
        {
            return ApiResponse<ThingResponse>.Fail("Color must be one of the 7 allowed values.", 400);
        }

        thing.Color = request.Color;
        thing.Price = RoundPrice(request.Price);
        thing.Number = request.Number;
        thing.Description = request.Description;

        await _dbContext.SaveChangesAsync();

        return ApiResponse<ThingResponse>.Success(ThingResponse.FromEntity(thing), "Updated.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var thing = await _dbContext.Things.FindAsync(id);
        if (thing == null)
        {
            return ApiResponse<bool>.Fail("Thing not found.", 404);
        }

        _dbContext.Things.Remove(thing);
        await _dbContext.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "Deleted.");
    }

    private static bool IsValidColor(string? color)
    {
        return !string.IsNullOrWhiteSpace(color) && AllowedColors.Contains(color);
    }

    private static decimal? RoundPrice(decimal? price)
    {
        return price.HasValue ? Math.Round(price.Value, 2, MidpointRounding.AwayFromZero) : null;
    }
}
