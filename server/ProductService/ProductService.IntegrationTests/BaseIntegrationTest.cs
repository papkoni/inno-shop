using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Persistence;

namespace ProductService.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
{
    protected readonly IntegrationTestWebAppFactory Factory;

    private readonly IServiceScope _scope;
    protected readonly ISender Sender;
    protected readonly ProductServiceDbContext DbContext;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        Factory = factory; 

        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<ProductServiceDbContext>();
    }
}