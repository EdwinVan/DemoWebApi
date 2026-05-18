using DemoWebApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemoWebApi.Data;

/// <summary>
/// 数据库上下文
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Thing> Things { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserName).HasColumnName("user_name").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Role).HasColumnName("role").HasConversion<int>();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.UserName).IsUnique();
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("room");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ComputerId).HasColumnName("computer_id");
            entity.Property(e => e.BedId).HasColumnName("bed_id");
        });

        modelBuilder.Entity<Thing>(entity =>
        {
            entity.ToTable("thing");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Color).HasColumnName("color").HasMaxLength(255);
            entity.Property(e => e.Price).HasColumnName("price").HasPrecision(10, 2);
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255);
        });
    }
}
