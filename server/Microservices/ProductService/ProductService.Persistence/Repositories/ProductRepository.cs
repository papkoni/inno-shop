using Microsoft.EntityFrameworkCore;
using ProductService.Application.Filters;
using ProductService.Application.Interfaces.DB;
using ProductService.Domain.Entities;

namespace ProductService.Persistence.Repositories;

public class ProductRepository: BaseRepository<Product>, IProductRepository
{
    private readonly ProductServiceDbContext _context;

    public ProductRepository(ProductServiceDbContext context) : base(context)
    {
        _context = context;
    }
    
    public async Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
    
    public async Task<List<Product>?> GetUserProductsAsync(
        Guid userId, 
        CancellationToken cancellationToken)
    {
        return await _context.Products
            .Where(p => p.CreatedByUserId == userId)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<List<Product>?> GetFilteredProductsAsync(
        ProductFilter filter, 
        CancellationToken cancellationToken,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Title))
            query = query.Where(p => p.Title.Contains(filter.Title));

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice);

        if (filter.CreatedByUserId.HasValue)
            query = query.Where(p => p.CreatedByUserId == filter.CreatedByUserId);

        return await query
            .OrderBy(p => p.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}