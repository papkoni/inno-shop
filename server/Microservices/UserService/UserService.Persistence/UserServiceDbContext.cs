using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Persistence.Configurations;
using UserService.Persistence.Interfaces;

namespace UserService.Persistence;

public class UserServiceDbContext: DbContext, IUserServiceDbContext
{
    public UserServiceDbContext(DbContextOptions<UserServiceDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
    }
} 