using MediatR;
using ProductService.Domain.Entities;

namespace ProductService.Application.Handlers.Queries.Product.GetUserProducts;

public record GetUserProductsQuery(string? UserIdClaim): IRequest<List<Domain.Entities.Product>>;