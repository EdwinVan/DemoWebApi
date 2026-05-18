using DemoWebApi.Data;
using DemoWebApi.Models.Dtos;
using DemoWebApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemoWebApi.Services;

public class RoomService : IRoomService
{
    private readonly AppDbContext _dbContext;

    public RoomService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<RoomResponse>> CreateAsync(CreateRoomRequest request)
    {
        var validation = await ValidateThingIdsAsync(request.ComputerId, request.BedId);
        if (validation != null)
        {
            return ApiResponse<RoomResponse>.Fail(validation, 400);
        }

        var room = new Room
        {
            ComputerId = request.ComputerId,
            BedId = request.BedId
        };

        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync();

        return ApiResponse<RoomResponse>.Success(RoomResponse.FromEntity(room), "Room created.");
    }

    public async Task<ApiResponse<RoomResponse?>> GetByIdAsync(byte id)
    {
        var room = await _dbContext.Rooms.FindAsync(id);
        if (room == null)
        {
            return ApiResponse<RoomResponse?>.Fail("Room not found.", 404);
        }

        return ApiResponse<RoomResponse?>.Success(RoomResponse.FromEntity(room));
    }

    public async Task<ApiResponse<List<RoomResponse>>> GetAllAsync()
    {
        var rooms = await _dbContext.Rooms
            .OrderBy(r => r.Id)
            .ToListAsync();

        return ApiResponse<List<RoomResponse>>.Success(rooms.Select(RoomResponse.FromEntity).ToList());
    }

    public async Task<ApiResponse<RoomThingsResponse>> GetThingsAsync(byte id)
    {
        var room = await _dbContext.Rooms.FindAsync(id);
        if (room == null)
        {
            return ApiResponse<RoomThingsResponse>.Fail("Room not found.", 404);
        }

        var thingIds = new[] { room.ComputerId, room.BedId }
            .Where(thingId => thingId.HasValue)
            .Select(thingId => thingId!.Value)
            .Distinct()
            .ToList();

        var things = await _dbContext.Things
            .Where(t => thingIds.Contains(t.Id))
            .OrderBy(t => t.Id)
            .ToListAsync();

        var response = new RoomThingsResponse
        {
            Room = RoomResponse.FromEntity(room),
            Things = things.Select(ThingResponse.FromEntity).ToList()
        };

        return ApiResponse<RoomThingsResponse>.Success(response);
    }

    public async Task<ApiResponse<RoomResponse>> UpdateAsync(byte id, UpdateRoomRequest request)
    {
        var room = await _dbContext.Rooms.FindAsync(id);
        if (room == null)
        {
            return ApiResponse<RoomResponse>.Fail("Room not found.", 404);
        }

        var validation = await ValidateThingIdsAsync(request.ComputerId, request.BedId);
        if (validation != null)
        {
            return ApiResponse<RoomResponse>.Fail(validation, 400);
        }

        room.ComputerId = request.ComputerId;
        room.BedId = request.BedId;

        await _dbContext.SaveChangesAsync();

        return ApiResponse<RoomResponse>.Success(RoomResponse.FromEntity(room), "Room updated.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(byte id)
    {
        var room = await _dbContext.Rooms.FindAsync(id);
        if (room == null)
        {
            return ApiResponse<bool>.Fail("Room not found.", 404);
        }

        _dbContext.Rooms.Remove(room);
        await _dbContext.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "Room deleted.");
    }

    private async Task<string?> ValidateThingIdsAsync(int? computerId, int? bedId)
    {
        var ids = new[] { computerId, bedId }
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return null;
        }

        var existingIds = await _dbContext.Things
            .Where(t => ids.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync();

        var missing = ids.Except(existingIds).ToList();
        if (missing.Count > 0)
        {
            return $"Thing id not found: {string.Join(", ", missing)}";
        }

        return null;
    }
}
