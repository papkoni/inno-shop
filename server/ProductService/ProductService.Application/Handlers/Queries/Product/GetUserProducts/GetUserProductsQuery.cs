using MediatR;
using ProductService.Domain.Entities;

namespace ProductService.Application.Handlers.Queries.Product.GetUserProducts;

public record GetUserProductsQuery(Guid CreatedByUserId): IRequest<List<Domain.Entities.Product>>;