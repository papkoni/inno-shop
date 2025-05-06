using Mapster;
using ProductService.Application.Handlers.Commands.Product.CreateProduct;
using ProductService.Domain.Entities;

namespace ProductService.Application.Mapping;

public class MappingConfigCreateProduct: IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateProductCommand, Product>()
            .ConstructUsing(src => new Product(
                src.CreateParameters.Title,
                src.CreateParameters.Description,
                src.CreateParameters.Price,
                Guid.Parse(src.UserIdClaim),
                true));
    }
}