namespace UserService.Infrastructure.Authentication;

public class JwtOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public int AccessTokenExpiresMinutes { get; set; } 
    public int RefreshTokenExpiresMinutes { get; set; }
}