using MediatR;
using ProductService.Application.DTOs.Product;

namespace ProductService.Application.Handlers.Commands.Product.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    UpdateProductDto UpdateParameters): IRequest<Guid>;