namespace UserService.Domain.Entities;

public class RefreshToken
{
    protected RefreshToken() { }
    
    public RefreshToken(
        string token, 
        int expiryDate,
        Guid? userId)
    {
        Id = Guid.NewGuid();
        Token = token;
        CreatedDate = DateTime.UtcNow;
        ExpiryDate = CreatedDate.AddMinutes(expiryDate);
        UserId = userId;
        IsRevoked = false; 
    }
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime ExpiryDate { get; set; } 
    public bool IsRevoked { get; set; } = false; 
    
    public virtual User User { get; set; } = null!;
}