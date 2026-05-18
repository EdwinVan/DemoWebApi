using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DemoWebApi.Data;
using DemoWebApi.Models.Configs;
using DemoWebApi.Models.Dtos;
using DemoWebApi.Models.Entities;
using DemoWebApi.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DemoWebApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;

    public AuthService(AppDbContext dbContext, IOptions<JwtOptions> jwtOptions)
    {
        _dbContext = dbContext;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserNameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ApiResponse<LoginResponse>.Fail("用户名/邮箱和密码不能为空", 400);
        }

        var input = request.UserNameOrEmail.Trim();
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.UserName == input || u.Email == input);

        if (user == null || !PasswordSecurity.VerifyPassword(request.Password, user.PasswordHash))
        {
            return ApiResponse<LoginResponse>.Fail("用户名或密码错误", 401);
        }

        if (!user.IsActive)
        {
            return ApiResponse<LoginResponse>.Fail("账号已禁用", 403);
        }

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_jwtOptions.ExpireMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: creds);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        user.LastLoginAt = DateTime.Now;
        await _dbContext.SaveChangesAsync();

        var response = new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = UserResponse.FromEntity(user)
        };

        return ApiResponse<LoginResponse>.Success(response, "登录成功");
    }

    public async Task<ApiResponse<UserResponse>> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return ApiResponse<UserResponse>.Fail("用户名、邮箱、密码不能为空", 400);
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
            Role = UserRole.NormalUser,
            IsActive = true
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return ApiResponse<UserResponse>.Success(UserResponse.FromEntity(user), "注册成功");
    }

    public async Task<ApiResponse<UserResponse>> GetCurrentUserAsync(int userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return ApiResponse<UserResponse>.Fail("用户不存在", 404);
        }

        return ApiResponse<UserResponse>.Success(UserResponse.FromEntity(user));
    }
}
