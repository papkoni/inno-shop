using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.DTOs;
using UserService.Application.Handlers.Commands.Auth.Login;
using UserService.Application.Handlers.Commands.Auth.RefreshTokens;
using UserService.Application.Handlers.Commands.Auth.RevokeToken;
using UserService.Application.Handlers.Commands.Users.UserRegistration;
using UserService.Domain.Exceptions;

namespace UserService.IntegrationTests.Application.Command.Auth;

public class RevokeTokenTests: BaseIntegrationTest
{
    public RevokeTokenTests(IntegrationTestWebAppFactory factory) 
        : base(factory) { }
    
    [Fact]
    public async Task RevokeToken_ShouldRevokeRefreshToken_WhenUserExists()
    {
        // Arrange
        var email = "revoke-test@example.com";
        var password = "Password123";
        var name = "Revoke Test User";
        
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
            await sender.Send(loginCommand);
            
            var user = DbContext.Users.FirstOrDefault(u => u.Email == email);
            userId = user.Id;
        }
        
        // Act
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var revokeCommand = new RevokeTokenCommand(userId);
            await sender.Send(revokeCommand);
        }
        
        // Assert
        var refreshToken = DbContext.RefreshTokens.FirstOrDefault(rt => rt.UserId == userId);
        Assert.NotNull(refreshToken);
        Assert.True(refreshToken.IsRevoked, "Токен должен быть отозван");
    }
    
    [Fact]
    public async Task RevokeToken_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        var command = new RevokeTokenCommand(nonExistentUserId);
        
        // Act & Assert
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            await Assert.ThrowsAsync<NotFoundException>(() => 
                sender.Send(command));
        }
    }
    
    [Fact]
    public async Task RevokeToken_ShouldRevokeAlreadyRevokedToken_WithoutError()
    {
        // Arrange
        var email = "double-revoke@example.com";
        var password = "Password123";
        var name = "Double Revoke Test";
        
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
            await sender.Send(loginCommand);

            var user = DbContext.Users.FirstOrDefault(u => u.Email == email);
            userId = user.Id;
        }

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var revokeCommand = new RevokeTokenCommand(userId);
            await sender.Send(revokeCommand);
        }
        
        // Act
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var revokeCommand = new RevokeTokenCommand(userId);
            await sender.Send(revokeCommand);
        }
        
        // Assert
        var refreshToken = DbContext.RefreshTokens.FirstOrDefault(rt => rt.UserId == userId);
        Assert.NotNull(refreshToken);
        Assert.True(refreshToken.IsRevoked, "Токен должен быть отозван");
    }
    
    [Fact]
    public async Task RevokeToken_ShouldMakeRefreshTokenInvalid()
    {
        // Arrange
        var email = "invalid-after-revoke@example.com";
        var password = "Password123";
        var name = "Invalid After Revoke Test";
        
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
        }
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var user = DbContext.Users.FirstOrDefault(u => u.Email == email);
            userId = user.Id;
            
            var revokeCommand = new RevokeTokenCommand(userId);
            await sender.Send(revokeCommand);
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