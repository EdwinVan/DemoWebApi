using DemoWebApi.Data;
using DemoWebApi.Models.Dtos;
using DemoWebApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemoWebApi.Services;

/// <summary>
/// 用户服务实现
/// </summary>
public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<UserResponse>> CreateAsync(CreateUserRequest request)
    {
        // 检查邮箱是否已存在
        if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
        {
            return ApiResponse<UserResponse>.Fail("邮箱已存在", 400);
        }

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            Age = request.Age
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return ApiResponse<UserResponse>.Success(UserResponse.FromEntity(user), "创建成功");
    }

    public async Task<ApiResponse<UserResponse?>> GetByIdAsync(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null)
        {
            return ApiResponse<UserResponse?>.Fail("用户不存在", 404);
        }

        return ApiResponse<UserResponse?>.Success(UserResponse.FromEntity(user));
    }

    public async Task<ApiResponse<List<UserResponse>>> GetAllAsync()
    {
        var users = await _dbContext.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        var list = users.Select(UserResponse.FromEntity).ToList();
        return ApiResponse<List<UserResponse>>.Success(list);
    }

    public async Task<ApiResponse<UserResponse>> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null)
        {
            return ApiResponse<UserResponse>.Fail("用户不存在", 404);
        }

        // 检查邮箱是否被其他用户占用
        if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
        {
            return ApiResponse<UserResponse>.Fail("邮箱已被其他用户使用", 400);
        }

        user.UserName = request.UserName;
        user.Email = request.Email;
        user.Age = request.Age;
        user.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync();

        return ApiResponse<UserResponse>.Success(UserResponse.FromEntity(user), "更新成功");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null)
        {
            return ApiResponse<bool>.Fail("用户不存在", 404);
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "删除成功");
    }
}
