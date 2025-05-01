using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.DB;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken); 
}