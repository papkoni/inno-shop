using MediatR;
using ProductService.Application.DTOs.Product;

namespace ProductService.Application.Handlers.Commands.Product.CreateProduct;

public record CreateProductCommand(
    CreateProductDto CreateParameters,
    string? UserIdClaim): IRequest<Guid>;
