using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.DB;

public interface IRefreshTokenRepository: IBaseRepository<RefreshToken>
{
    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken); 
}