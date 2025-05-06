using Microsoft.EntityFrameworkCore;

namespace ProductService.Persistence.Interfaces;

public interface IProductServiceDbContext
{
    DbSet<T> Set<T>() where T : class;
}