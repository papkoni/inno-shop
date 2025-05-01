namespace UserService.Application.Interfaces.Security;

public interface IPasswordHasher
{
    string Generate(string password);
    bool Verify(string password, string hashPassword);
}