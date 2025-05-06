using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace UserService.Persistence.Extensions;

public static class DatabaseMigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateAsyncScope();

        using UserServiceDbContext productServiceDbContext =
            scope.ServiceProvider.GetRequiredService<UserServiceDbContext>();

        productServiceDbContext.Database.Migrate();
    }
}