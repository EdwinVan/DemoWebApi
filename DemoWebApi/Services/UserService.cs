using DemoWebApi.Data;
using DemoWebApi.Models.Dtos;
using DemoWebApi.Models.Entities;
using DemoWebApi.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace DemoWebApi.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<UserResponse>> CreateAsync(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) ||
            string.IsNullOrWhiteSpace(request.Email))
        {
            return ApiResponse<UserResponse>.Fail("用户名、邮箱不能为空", 400);
        }

        var passwordRule = PasswordPolicy.Validate(request.Password);
        if (!passwordRule.IsValid)
        {
            return ApiResponse<UserResponse>.Fail(passwordRule.Message, 400);
        }

        if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
        {
            return ApiResponse<UserResponse>.Fail("邮箱已存在", 400);
        }

        if (await _dbContext.Users.AnyAsync(u => u.UserName == request.UserName))
        {
            return ApiResponse<UserResponse>.Fail("用户名已存在", 400);
        }

        var user = new User
        {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            Age = request.Age,
            PasswordHash = PasswordSecurity.HashPassword(request.Password),
            Role = request.Role,
            IsActive = true
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
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<UserResponse>>.Success(users.Select(UserResponse.FromEntity).ToList());
    }

    public async Task<ApiResponse<UserResponse>> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null)
        {
            return ApiResponse<UserResponse>.Fail("用户不存在", 404);
        }

        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Email))
        {
            return ApiResponse<UserResponse>.Fail("用户名和邮箱不能为空", 400);
        }

        if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
        {
            return ApiResponse<UserResponse>.Fail("邮箱已被其他用户使用", 400);
        }

        if (await _dbContext.Users.AnyAsync(u => u.UserName == request.UserName && u.Id != id))
        {
            return ApiResponse<UserResponse>.Fail("用户名已被其他用户使用", 400);
        }

        user.UserName = request.UserName.Trim();
        user.Email = request.Email.Trim();
        user.Age = request.Age;
        user.IsActive = request.IsActive;
        user.Role = request.Role;

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            var passwordRule = PasswordPolicy.Validate(request.NewPassword);
            if (!passwordRule.IsValid)
            {
                return ApiResponse<UserResponse>.Fail(passwordRule.Message, 400);
            }

            user.PasswordHash = PasswordSecurity.HashPassword(request.NewPassword);
        }

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

        if (user.Role == UserRole.SuperAdmin)
        {
            return ApiResponse<bool>.Fail("超级管理员账号不允许删除", 400);
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "删除成功");
    }
}
