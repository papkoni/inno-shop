using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.DB;

public interface IUserRepository: IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}