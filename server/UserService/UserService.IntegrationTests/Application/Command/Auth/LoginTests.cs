using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.DTOs;
using UserService.Application.Handlers.Commands.Auth.Login;
using UserService.Application.Handlers.Commands.Users.UserRegistration;
using UserService.Domain.Exceptions;

namespace UserService.IntegrationTests.Application.Command.Auth;

public class LoginTests: BaseIntegrationTest
{
    public LoginTests(IntegrationTestWebAppFactory factory) 
        : base(factory) { }
    
    [Fact]
    public async Task Login_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        // Arrange
        var email = "login-test@example.com";
        var password = "Password123";
        var name = "Login Test User";
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            var registerCommand = new UserRegistrationCommand(
                email,
                password,
                name
            );
            
            await sender.Send(registerCommand);
        }
        
        // Act
        TokensDto result;
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var loginCommand = new LoginCommand(email, password);
            result = await sender.Send(loginCommand);
        }
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        
        var user = DbContext.Users.FirstOrDefault(u => u.Email == email);
        var refreshToken = DbContext.RefreshTokens.FirstOrDefault(rt => rt.UserId == user.Id);
        Assert.NotNull(refreshToken);
        Assert.Equal(result.RefreshToken, refreshToken.Token);
        Assert.False(refreshToken.IsRevoked);
    }
    
    [Fact]
    public async Task Login_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var nonExistentEmail = "nonexistent@example.com";
        var loginCommand = new LoginCommand(nonExistentEmail, "Password123");
        
        // Act & Assert
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            await Assert.ThrowsAsync<NotFoundException>(() => 
                sender.Send(loginCommand));
        }
    }
    
    [Fact]
    public async Task Login_ShouldThrowUnauthorizedException_WhenPasswordIsIncorrect()
    {
        // Arrange
        var email = "wrong-password@example.com";
        var password = "CorrectPassword123";
        var name = "Wrong Password Test";
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            var registerCommand = new UserRegistrationCommand(
                email,
                password,
                name
            );
            
            await sender.Send(registerCommand);
        }
        
        // Act & Assert
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var loginCommand = new LoginCommand(email, "WrongPassword123");
            
            await Assert.ThrowsAsync<UnauthorizedException>(() => 
                sender.Send(loginCommand));
        }
    }
    
    [Fact]
    public async Task Login_ShouldCreateNewRefreshToken_WhenLoginAgain()
    {
        // Arrange
        var email = "refresh-login@example.com";
        var password = "Password123";
        var name = "Refresh Login Test";
        
        string firstRefreshToken;

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var registerCommand = new UserRegistrationCommand(
                email,
                password,
                name
            );

            await sender.Send(registerCommand);
        }

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var loginCommand = new LoginCommand(email, password);
            var loginResult = await sender.Send(loginCommand);
            firstRefreshToken = loginResult.RefreshToken;
        }
        
        // Act
        TokensDto secondLoginResult;
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var loginCommand = new LoginCommand(email, password);
            secondLoginResult = await sender.Send(loginCommand);
        }
        
        // Assert
        Assert.NotEqual(firstRefreshToken, secondLoginResult.RefreshToken);
        
        var user = DbContext.Users.FirstOrDefault(u => u.Email == email);
        var refreshToken = DbContext.RefreshTokens.FirstOrDefault(rt => rt.UserId == user.Id);
        Assert.NotNull(refreshToken);
        Assert.Equal(secondLoginResult.RefreshToken, refreshToken.Token);
    }
}