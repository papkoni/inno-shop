using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Interfaces;
using ProductService.Application.Interfaces.DB;
using ProductService.Persistence.Interfaces;
using ProductService.Persistence.Repositories;

namespace ProductService.Persistence.Extensions;

public static class ServiceCollectionExtension
{
    public static void AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddDbContext<IProductServiceDbContext,ProductServiceDbContext>(
            options =>
            {
                options.UseNpgsql(configuration.GetConnectionString(nameof(ProductServiceDbContext)));
            }
        );
    }
}