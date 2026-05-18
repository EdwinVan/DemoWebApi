using DemoWebApi.Models.Dtos;

namespace DemoWebApi.Services;

public interface IThingService
{
    Task<ApiResponse<ThingResponse>> CreateAsync(CreateThingRequest request);
    Task<ApiResponse<ThingResponse?>> GetByIdAsync(int id);
    Task<ApiResponse<List<ThingResponse>>> GetAllAsync();
    Task<ApiResponse<ThingResponse>> UpdateAsync(int id, UpdateThingRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
