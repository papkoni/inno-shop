using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ProductService.Persistence.Extensions;

public static class DatabaseMigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateAsyncScope();

        using ProductServiceDbContext productServiceDbContext =
            scope.ServiceProvider.GetRequiredService<ProductServiceDbContext>();

        productServiceDbContext.Database.Migrate();
    }
}