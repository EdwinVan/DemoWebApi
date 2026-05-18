using DemoWebApi.Models.Dtos;

namespace DemoWebApi.Services;

public interface IUserService
{
    Task<ApiResponse<UserResponse>> CreateAsync(CreateUserRequest request);
    Task<ApiResponse<UserResponse?>> GetByIdAsync(int id);
    Task<ApiResponse<List<UserResponse>>> GetAllAsync();
    Task<ApiResponse<UserResponse>> UpdateAsync(int id, UpdateUserRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
