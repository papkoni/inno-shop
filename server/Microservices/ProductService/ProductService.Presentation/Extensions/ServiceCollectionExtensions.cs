using ProductService.Application.Handlers.Commands.Product.CreateProduct;

namespace ProductService.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddPresentation(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg
            .RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));
    }
}