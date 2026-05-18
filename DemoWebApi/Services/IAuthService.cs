using DemoWebApi.Models.Dtos;

namespace DemoWebApi.Services;

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<UserResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<UserResponse>> GetCurrentUserAsync(int userId);
}
