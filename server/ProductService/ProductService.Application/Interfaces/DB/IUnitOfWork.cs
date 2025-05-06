using ProductService.Application.Interfaces.DB;

namespace ProductService.Application.Interfaces.DB;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
    IProductRepository ProductRepository { get; }
}