using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Interfaces.DB;
using UserService.Persistence.Interfaces;
using UserService.Persistence.Repositories;

namespace UserService.Persistence.Extensions;

public static class ServiceCollectionExtension
{
    public static void AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddDbContext<IUserServiceDbContext,UserServiceDbContext>(
            options =>
            {
                var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
                       ?? configuration.GetConnectionString("UserServiceDbContext");
                options.UseNpgsql(connectionString);
            }
        );
    }
}