using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UserService.Persistence;

namespace UserService.IntegrationTests;

public class BaseIntegrationTest: IClassFixture<IntegrationTestWebAppFactory>
{
    protected readonly IntegrationTestWebAppFactory Factory;

    private readonly IServiceScope _scope;
    protected readonly ISender Sender;
    protected readonly UserServiceDbContext DbContext;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        Factory = factory; 

        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<UserServiceDbContext>();
    }
}