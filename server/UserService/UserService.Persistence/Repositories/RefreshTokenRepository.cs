using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces.DB;
using UserService.Domain.Entities;

namespace UserService.Persistence.Repositories;

public class RefreshTokenRepository: BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    private readonly UserServiceDbContext _context;

    public RefreshTokenRepository(UserServiceDbContext context): base(context)
    {
        _context = context;
    }
    
    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
            .Include(rt => rt.User) 
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }
}