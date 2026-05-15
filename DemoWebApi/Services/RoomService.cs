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
        var room = new Room
        {
            ComputerId = request.ComputerId,
            BedId = request.BedId
        };

        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync();

        return ApiResponse<RoomResponse>.Success(RoomResponse.FromEntity(room), "创建成功");
    }

    public async Task<ApiResponse<RoomResponse?>> GetByIdAsync(byte id)
    {
        var room = await _dbContext.Rooms.FindAsync(id);
        if (room == null)
        {
            return ApiResponse<RoomResponse?>.Fail("房间不存在", 404);
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
            return ApiResponse<RoomThingsResponse>.Fail("房间不存在", 404);
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
            return ApiResponse<RoomResponse>.Fail("房间不存在", 404);
        }

        room.ComputerId = request.ComputerId;
        room.BedId = request.BedId;

        await _dbContext.SaveChangesAsync();

        return ApiResponse<RoomResponse>.Success(RoomResponse.FromEntity(room), "更新成功");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(byte id)
    {
        var room = await _dbContext.Rooms.FindAsync(id);
        if (room == null)
        {
            return ApiResponse<bool>.Fail("房间不存在", 404);
        }

        _dbContext.Rooms.Remove(room);
        await _dbContext.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "删除成功");
    }
}
