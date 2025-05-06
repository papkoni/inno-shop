using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces.DB;
using UserService.Domain.Entities;

namespace UserService.Persistence.Repositories;

public class UserRepository: BaseRepository<User>, IUserRepository
{
    private readonly UserServiceDbContext _context;

    public UserRepository(UserServiceDbContext context): base(context)
    {
        _context = context;
    }
    
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return  await _context.Users
            .AsNoTracking()
            .Include(u => u.RefreshToken)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
    
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.RefreshToken)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
}