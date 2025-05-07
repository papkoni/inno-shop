using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Interfaces.Authentication;
using UserService.Application.Interfaces.ProductService;
using UserService.Application.Interfaces.Security;
using UserService.Infrastructure.Authentication;
using UserService.Infrastructure.ProductService;
using UserService.Infrastructure.Security;

namespace UserService.Infrastructure.Extensions;

public static class ServiceCollectionExtension
{
    public static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));
        
        services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client => 
        {
            var baseUrl = Environment.GetEnvironmentVariable("PRODUCTS_APP_PORT") 
                          ?? configuration["ProductService:BaseUrl"];
    
            client.BaseAddress = new Uri(baseUrl);
        });
    }
}