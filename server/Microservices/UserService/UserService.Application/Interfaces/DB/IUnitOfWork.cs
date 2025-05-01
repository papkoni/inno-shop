namespace UserService.Application.Interfaces.DB;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
    IUserRepository UserRepository { get; }
    IRefreshTokenRepository RefreshTokenRepository { get; }
}