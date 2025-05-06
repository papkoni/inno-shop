using MediatR;

namespace ProductService.Application.Handlers.Commands.Product.CreateProduct;

public record CreateProductCommand(
    string Title,
    string Description,
    decimal Price,
    Guid CreatedByUserId): IRequest<Guid>;
