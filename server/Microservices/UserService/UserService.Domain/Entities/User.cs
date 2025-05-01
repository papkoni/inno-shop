using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

public class User
{
    public User(Guid id,
        string name,
        string passwordHash,
        string email, 
        Role role,
        bool isActive = true)
    {
        Id = id;
        Name = name;
        PasswordHash = passwordHash;
        Email = email;
        Role = role;
        IsActive = isActive;
    }
    
    protected User(){}
    
    public Guid Id { get; set; }
    public string Name { get;  set; } = string.Empty;
    public string PasswordHash { get;  set; } = string.Empty;
    public string Email { get;  set; } = string.Empty;
    public Role Role { get;  set; }
    public bool IsActive { get; set; }
    
    public RefreshToken? RefreshToken { get; set; }
}