using ProductService.Application.Filters;
using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces.DB;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken);

    Task<List<Product>?> GetUserProductsAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<List<Product>?> GetFilteredProductsAsync(
        ProductFilter filter,
        CancellationToken cancellationToken,
        int pageNumber = 1,
        int pageSize = 10);
}