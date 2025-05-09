using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.DTOs;
using UserService.Application.Handlers.Commands.Users.UpdateUser;
using UserService.Application.Handlers.Commands.Users.UserRegistration;
using UserService.Application.Handlers.Queries.Users.GetUserById;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Domain.Exceptions;

namespace UserService.IntegrationTests.Application.Queries.Users;

public class GetUserByIdTests: BaseIntegrationTest
{
    public GetUserByIdTests(IntegrationTestWebAppFactory factory) 
        : base(factory) { }
    
    [Fact]
    public async Task GetUserById_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var email = "get-user-test@example.com";
        var password = "Password123";
        var name = "Get User Test";
        
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
            
            var user = DbContext.Users.FirstOrDefault(u => u.Email == email);
            userId = user.Id;
        }
        
        // Act
        UserByIdResponse result;
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var query = new GetUserByIdQuery(userId.ToString());
            result = await sender.Send(query);
        }
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal(email, result.Email);
        Assert.Equal(name, result.Name);
        Assert.Equal(Role.User.ToString(), result.Role);
    }
    
    [Fact]
    public async Task GetUserById_ShouldThrowUnauthorizedException_WhenUserIdClaimIsNull()
    {
        // Arrange
        string? userIdClaim = null;
        var query = new GetUserByIdQuery(userIdClaim);
        
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            Sender.Send(query));
    }
    
    [Fact]
    public async Task GetUserById_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        var query = new GetUserByIdQuery(nonExistentUserId.ToString());
        
        // Act & Assert
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            await Assert.ThrowsAsync<NotFoundException>(() => 
                sender.Send(query));
        }
    }
    
    [Fact]
    public async Task GetUserById_ShouldReturnIsActiveStatus_WhenUserIsDeactivated()
    {
        // Arrange
        var email = "active-test@example.com";
        var password = "Password123";
        var name = "Active Test User";
        
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
            
            var user = DbContext.Users.FirstOrDefault(u => u.Email == email);
            userId = user.Id;
            
            user.IsActive = false;
            DbContext.SaveChanges();
        }
        
        // Act
        UserByIdResponse result;
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var query = new GetUserByIdQuery(userId.ToString());
            result = await sender.Send(query);
        }
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.False(result.IsActive);
    }
}