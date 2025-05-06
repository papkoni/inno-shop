using ProductService.Application.DTOs.Product;
using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces.DB;

public interface IProductRepository: IBaseRepository<Product>
{
    Task<Product?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken);

    Task<List<Product>> GetUserProductsAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<List<Product>?> GetFilteredUserProductsAsync(
        Guid createdByUserId,
        ProductFilterDto filterDto,
        CancellationToken cancellationToken,
        int pageNumber = 1,
        int pageSize = 10);
}