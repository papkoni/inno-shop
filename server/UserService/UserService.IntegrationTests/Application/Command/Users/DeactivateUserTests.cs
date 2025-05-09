using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Handlers.Commands.Users.DeactivateUser;
using UserService.Application.Handlers.Commands.Users.UserRegistration;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using Moq;
using UserService.Application.Interfaces.ProductService;
using UserService.Persistence;

namespace UserService.IntegrationTests.Application.Command.Users;

public class DeactivateUserTests: BaseIntegrationTest
{
    private readonly Mock<IProductServiceClient> _mockProductServiceClient;
    
    public DeactivateUserTests(IntegrationTestWebAppFactory factory) 
        : base(factory) 
    {
        _mockProductServiceClient = new Mock<IProductServiceClient>();
        
        // Регистрируем мок IProductServiceClient в DI контейнере
        var services = factory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope().ServiceProvider;
        var productServiceClient = services.GetService<IProductServiceClient>();
        if (productServiceClient != null)
        {
            _mockProductServiceClient.Setup(m => m.NotifyUserDeactivationAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }
    }
    
    [Fact]
    public async Task DeactivateUser_ShouldDeactivateUser_WhenUserExists()
    {
        // Arrange
        var userEmail = "deactivate-test@example.com";
        var userName = "Deactivate Test";

        Guid userId;

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var registerCommand = new UserRegistrationCommand(userEmail, "Password123", userName);
            await sender.Send(registerCommand);

            var user = DbContext.Users.FirstOrDefault(u => u.Email == userEmail);
            userId = user.Id;
        }

        // Act
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var deactivateCommand = new DeactivateUserCommand(userId.ToString(), false);
            await sender.Send(deactivateCommand);
        }

        // Assert
        using (var scope = Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<UserServiceDbContext>();
            var deactivatedUser = dbContext.Users.FirstOrDefault(u => u.Id == userId);
    
            Assert.NotNull(deactivatedUser);
            Assert.False(deactivatedUser.IsActive);
        }
    }

    
    [Fact]
    public async Task DeactivateUser_ShouldReactivateUser_WhenUserExistsAndWasDeactivated()
    {
        // Arrange - создаем и деактивируем пользователя
        var userEmail = "reactivate-test@example.com";
        var userName = "Reactivate Test";
        
        Guid userId;

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            // Регистрируем пользователя
            var registerCommand = new UserRegistrationCommand(
                userEmail,
                "Password123",
                userName
            );

            await sender.Send(registerCommand);
        }

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        // Получаем ID созданного пользователя из БД
            var user = DbContext.Users.FirstOrDefault(u => u.Email == userEmail);
            userId = user.Id;
            
            // Деактивируем пользователя
            _mockProductServiceClient.Setup(m => m.NotifyUserDeactivationAsync(userId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            var deactivateCommand = new DeactivateUserCommand(userId.ToString(), false);
            await sender.Send(deactivateCommand);
        }
        
        // Act - реактивируем пользователя
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            var command = new DeactivateUserCommand(userId.ToString(), true);
            await sender.Send(command);
        }
        
        // Assert
        var reactivatedUser = DbContext.Users.FirstOrDefault(u => u.Id == userId);
        Assert.NotNull(reactivatedUser);
        Assert.True(reactivatedUser.IsActive);
    }
    
    [Fact]
    public async Task DeactivateUser_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        var command = new DeactivateUserCommand(nonExistentUserId.ToString(), false);
        
        // Act & Assert
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            await Assert.ThrowsAsync<NotFoundException>(() => 
                sender.Send(command));
        }
    }
    
    [Fact]
    public async Task DeactivateUser_ShouldThrowUnauthorizedException_WhenUserIdClaimIsNull()
    {
        // Arrange
        string? userIdClaim = null;
        var command = new DeactivateUserCommand(userIdClaim, false);
        
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            Sender.Send(command));
    }
    
    [Fact]
    public async Task DeactivateUser_ShouldThrowUnauthorizedException_WhenUserIdClaimIsEmpty()
    {
        // Arrange
        var userIdClaim = string.Empty;
        var command = new DeactivateUserCommand(userIdClaim, false);
        
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            Sender.Send(command));
    }
}