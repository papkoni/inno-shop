using ProductService.Application.Interfaces;
using ProductService.Application.Interfaces.DB;

namespace ProductService.Persistence;

public class UnitOfWork: IUnitOfWork
{
    private readonly ProductServiceDbContext _context;

    public UnitOfWork(ProductServiceDbContext context, IProductRepository productRepository)
    {
        _context = context;
        productRepository = ProductRepository;
    }

    public IProductRepository ProductRepository { get; }
    
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}