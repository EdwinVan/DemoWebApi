using DemoWebApi.Models.Dtos;

namespace DemoWebApi.Services;

public interface IRoomService
{
    Task<ApiResponse<RoomResponse>> CreateAsync(CreateRoomRequest request);
    Task<ApiResponse<RoomResponse?>> GetByIdAsync(byte id);
    Task<ApiResponse<List<RoomResponse>>> GetAllAsync();
    Task<ApiResponse<RoomThingsResponse>> GetThingsAsync(byte id);
    Task<ApiResponse<RoomResponse>> UpdateAsync(byte id, UpdateRoomRequest request);
    Task<ApiResponse<bool>> DeleteAsync(byte id);
}
