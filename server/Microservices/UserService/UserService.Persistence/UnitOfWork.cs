using UserService.Application.Interfaces.DB;

namespace UserService.Persistence;

public class UnitOfWork: IUnitOfWork
{
    private readonly UserServiceDbContext _context;

    public UnitOfWork(
        UserServiceDbContext context,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _context = context;
        UserRepository = userRepository;
        RefreshTokenRepository = refreshTokenRepository;
    }

    public IUserRepository UserRepository { get; }
    public IRefreshTokenRepository RefreshTokenRepository { get; }
    
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}