using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.DTOs;
using UserService.Application.Interfaces.Authentication;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;

namespace UserService.Infrastructure.Authentication;

public class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _options;

    public JwtProvider(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }
    
    public TokensDto GenerateTokens(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user.Id);
        
        return new TokensDto(accessToken, refreshToken);
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),  
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) 
        };

        return GenerateJwtToken(claims, _options.AccessTokenExpiresMinutes);
    }

    public string GenerateRefreshToken(Guid userId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) 
        };

        return GenerateJwtToken(claims, _options.RefreshTokenExpiresMinutes);
        
    }
    
    public int GetRefreshTokenExpirationMinutes()
    {
        return _options.RefreshTokenExpiresMinutes;
    }
    
    public bool ValidateRefreshToken(string refreshToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_options.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidAlgorithms = new[] {SecurityAlgorithms.HmacSha256}
            };

            tokenHandler.ValidateToken(refreshToken, validationParameters, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken)
            {
                throw new SecurityTokenException("Invalid JWT format.");
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
            
            return false;
        }
    }
    
    public Guid GetUserIdFromRefreshToken(string refreshToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var jwtToken = tokenHandler.ReadJwtToken(refreshToken);

            var userIdClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null)
            {
                throw new InvalidTokenException("UserId claim not found in the refresh token.");
            }

            return Guid.Parse(userIdClaim.Value);
        }
        catch (SecurityTokenException ex)
        {
            throw new InvalidTokenException($"Invalid refresh token: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new InvalidTokenException("Error extracting userId from refresh token.");
        }
    }

    private string GenerateJwtToken(Claim[] claims, double expiresIn)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Claims = claims.ToDictionary(c => c.Type, c => (object)c.Value),
            Expires = DateTime.UtcNow.AddMinutes(expiresIn),
            SigningCredentials = credentials,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        return tokenHandler.WriteToken(token);
    }
}