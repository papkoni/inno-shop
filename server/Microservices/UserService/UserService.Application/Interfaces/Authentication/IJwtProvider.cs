using UserService.Application.DTOs;
using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Authentication;

public interface IJwtProvider
{ 
    TokensDto GenerateTokens(User user);
    string GenerateAccessToken(User user);
    string GenerateRefreshToken(Guid userId);
    int GetRefreshTokenExpirationMinutes();
    Guid GetUserIdFromRefreshToken(string refreshToken);
    bool ValidateRefreshToken(string refreshToken);
}