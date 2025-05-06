using Microsoft.EntityFrameworkCore;
using ProductService.Application.DTOs.Product;
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
    
    public async Task<List<Product>> GetUserProductsAsync(
        Guid userId, 
        CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.CreatedByUserId == userId)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<List<Product>?> GetFilteredUserProductsAsync(
        Guid createdByUserId,
        ProductFilterDto filterDto, 
        CancellationToken cancellationToken,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filterDto.Title))
            query = query.Where(p => p.Title.Contains(filterDto.Title));

        if (filterDto.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filterDto.MinPrice);

        if (filterDto.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filterDto.MaxPrice);
        
        return await query
            .AsNoTracking()
            .Where(p => p.CreatedByUserId == createdByUserId)
            .OrderBy(p => p.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}