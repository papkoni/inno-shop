using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Persistence.Configurations;
using ProductService.Persistence.Interfaces;

namespace ProductService.Persistence;

public class ProductServiceDbContext: DbContext, IProductServiceDbContext
{
    public ProductServiceDbContext(DbContextOptions<ProductServiceDbContext> options) : base(options)
    {
    }
    
    public DbSet<Product> Products { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProductEntityConfiguration());
    }
} 