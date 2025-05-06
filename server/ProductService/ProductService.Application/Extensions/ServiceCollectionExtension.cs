using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Mapster;
using MediatR;
using ProductService.Application.Behaviors;
using ProductService.Application.Handlers.Commands.Product.CreateProduct;
using ProductService.Application.Mapping;

namespace ProductService.Application.Extensions;

public static class ServiceCollectionExtension
{
    public static void AddApplication(
        this IServiceCollection services
        )
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(MappingConfigCreateProduct).Assembly);
        
        services.AddMapster(); 
        services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>)
        );
    }
}