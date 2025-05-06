using MediatR;
using ProductService.Application.DTOs.Product;
namespace ProductService.Application.Handlers.Queries.Product.GetFilteredUserProducts;

public record GetFilteredUserProductsQuery(
    string? UserIdClaim,
    ProductFilterDto Filter,
    int PageNumber,
    int PageSize): IRequest<List<Domain.Entities.Product>>;