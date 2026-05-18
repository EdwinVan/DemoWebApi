using DemoWebApi.Models.Configs;
using DemoWebApi.Models.Entities;
using DemoWebApi.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DemoWebApi.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext dbContext, IOptions<SuperAdminOptions> superAdminOptions)
    {
        await dbContext.Database.EnsureCreatedAsync();
        await EnsureUsersTableAsync(dbContext);
        await EnsureUserAuthColumnsAsync(dbContext);
        await EnsureSuperAdminAsync(dbContext, superAdminOptions.Value);
    }

    private static async Task EnsureUsersTableAsync(AppDbContext dbContext)
    {
        const string sql = @"
SELECT COUNT(*) AS Value
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'users'";

        var exists = await dbContext.Database.SqlQueryRaw<int>(sql).SingleAsync();
        if (exists > 0)
        {
            return;
        }

        const string createSql = @"
CREATE TABLE users (
    id int NOT NULL AUTO_INCREMENT,
    user_name varchar(50) NOT NULL,
    email varchar(100) NOT NULL,
    age int NOT NULL,
    password_hash varchar(255) NOT NULL,
    role int NOT NULL DEFAULT 0,
    created_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_active tinyint(1) NOT NULL DEFAULT 1,
    last_login_at datetime NULL,
    PRIMARY KEY (id),
    UNIQUE KEY UX_users_email (email),
    UNIQUE KEY UX_users_user_name (user_name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci";

        await dbContext.Database.ExecuteSqlRawAsync(createSql);
    }

    private static async Task EnsureUserAuthColumnsAsync(AppDbContext dbContext)
    {
        await EnsureColumnExistsAsync(dbContext, "users", "password_hash", "password_hash varchar(255) NULL");
        await EnsureColumnExistsAsync(dbContext, "users", "role", "role int NOT NULL DEFAULT 0");
        await EnsureColumnExistsAsync(dbContext, "users", "last_login_at", "last_login_at datetime NULL");

        var fallbackHash = PasswordSecurity.HashPassword("User@123456");
        await dbContext.Database.ExecuteSqlRawAsync(
            "UPDATE users SET password_hash = {0} WHERE password_hash IS NULL OR password_hash = ''",
            fallbackHash);
    }

    private static async Task EnsureColumnExistsAsync(
        AppDbContext dbContext,
        string tableName,
        string columnName,
        string addColumnDefinition)
    {
        var sql = $@"
SELECT COUNT(*) AS Value
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = '{tableName}'
  AND COLUMN_NAME = '{columnName}'";

        var exists = await dbContext.Database.SqlQueryRaw<int>(sql).SingleAsync();
        if (exists > 0)
        {
            return;
        }

        await dbContext.Database.ExecuteSqlRawAsync($"ALTER TABLE {tableName} ADD COLUMN {addColumnDefinition}");
    }

    private static async Task EnsureSuperAdminAsync(AppDbContext dbContext, SuperAdminOptions options)
    {
        var superAdmin = await dbContext.Users
            .FirstOrDefaultAsync(u => u.UserName == options.UserName || u.Email == options.Email);

        if (superAdmin == null)
        {
            superAdmin = new User
            {
                UserName = options.UserName,
                Email = options.Email,
                Age = options.Age,
                PasswordHash = PasswordSecurity.HashPassword(options.Password),
                Role = UserRole.SuperAdmin,
                IsActive = true
            };

            dbContext.Users.Add(superAdmin);
        }
        else
        {
            superAdmin.Role = UserRole.SuperAdmin;
            superAdmin.IsActive = true;
            superAdmin.PasswordHash = PasswordSecurity.HashPassword(options.Password);
        }

        await dbContext.SaveChangesAsync();
    }
}
