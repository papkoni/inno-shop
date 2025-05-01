namespace UserService.Application.Interfaces.DB;

public interface IBaseRepository<T> where T : class
{
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken);
    Task CreateAsync(T entity, CancellationToken cancellationToken);
    void Update(T entity);
    void Delete(T entity);
}