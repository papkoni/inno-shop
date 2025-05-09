using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.DTOs;
using UserService.Application.Handlers.Commands.Auth.Login;
using UserService.Application.Handlers.Commands.Auth.RefreshTokens;
using UserService.Application.Handlers.Commands.Users.UserRegistration;
using UserService.Domain.Exceptions;

namespace UserService.IntegrationTests.Application.Command.Auth;

public class RefreshTokenTests: BaseIntegrationTest
{
    public RefreshTokenTests(IntegrationTestWebAppFactory factory) 
        : base(factory) { }
    
    [Fact]
    public async Task RefreshToken_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
    {
        // Arrange
        var email = "refresh-test@example.com";
        var password = "Password123";
        var name = "Refresh Test User";
        
        TokensDto loginResult;
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
            loginResult = await sender.Send(loginCommand);
        }
        
        // Act
        TokensDto refreshResult;
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var refreshCommand = new RefreshTokensCommand(loginResult.RefreshToken);
            refreshResult = await sender.Send(refreshCommand);
        }
        
        // Assert
        Assert.NotNull(refreshResult);
        Assert.NotEmpty(refreshResult.AccessToken);
        Assert.NotEmpty(refreshResult.RefreshToken);
        Assert.NotEqual(loginResult.AccessToken, refreshResult.AccessToken);
        Assert.NotEqual(loginResult.RefreshToken, refreshResult.RefreshToken);
        
        var user = DbContext.Users.FirstOrDefault(u => u.Email == email);
        var refreshToken = DbContext.RefreshTokens.FirstOrDefault(rt => rt.UserId == user.Id);
        Assert.NotNull(refreshToken);
        Assert.Equal(refreshResult.RefreshToken, refreshToken.Token);
        Assert.False(refreshToken.IsRevoked);
    }
    
    [Fact]
    public async Task RefreshToken_ShouldThrowInvalidTokenException_WhenRefreshTokenIsInvalid()
    {
        // Arrange
        var invalidToken = "invalid.refresh.token";
        var command = new RefreshTokensCommand(invalidToken);
        
        // Act & Assert
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            await Assert.ThrowsAsync<InvalidTokenException>(() => 
                sender.Send(command));
        }
    }
    
    [Fact]
    public async Task RefreshToken_ShouldThrowInvalidTokenException_WhenTokenIsRevoked()
    {
        // Arrange
        var email = "revoked-token@example.com";
        var password = "Password123";
        var name = "Revoked Token Test";
        
        TokensDto loginResult;
        Guid userId;

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
            loginResult = await sender.Send(loginCommand);
            
            var user = DbContext.Users.FirstOrDefault(u => u.Email == email);
            userId = user.Id;
            
            var refreshToken = DbContext.RefreshTokens.FirstOrDefault(rt => rt.UserId == userId);
            refreshToken.IsRevoked = true;
            DbContext.SaveChanges();
        }
        
        // Act & Assert
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var refreshCommand = new RefreshTokensCommand(loginResult.RefreshToken);
            
            await Assert.ThrowsAsync<InvalidTokenException>(() => 
                sender.Send(refreshCommand));
        }
    }
    
    [Fact]
    public async Task RefreshToken_ShouldThrowInvalidTokenException_WhenTokenIsExpired()
    {
        // Arrange
        var email = "expired-token@example.com";
        var password = "Password123";
        var name = "Expired Token Test";
        
        TokensDto loginResult;
        Guid userId;

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
            loginResult = await sender.Send(loginCommand);
            
            var user = DbContext.Users.FirstOrDefault(u => u.Email == email);
            userId = user.Id;
            
            var refreshToken = DbContext.RefreshTokens.FirstOrDefault(rt => rt.UserId == userId);
            refreshToken.ExpiryDate = DateTime.UtcNow.AddMinutes(-10);
            DbContext.SaveChanges();
        }
        
        // Act & Assert
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var refreshCommand = new RefreshTokensCommand(loginResult.RefreshToken);
            
            await Assert.ThrowsAsync<InvalidTokenException>(() => 
                sender.Send(refreshCommand));
        }
    }
}