using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.DTOs;
using UserService.Application.Handlers.Commands.Users.UpdateUser;
using UserService.Application.Handlers.Commands.Users.UserRegistration;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Domain.Exceptions;
using FluentValidation;
using UserService.Persistence;

namespace UserService.IntegrationTests.Application.Command.Users;

public class UpdateUserTests: BaseIntegrationTest
{
    public UpdateUserTests(IntegrationTestWebAppFactory factory) 
        : base(factory) { }
    
    [Fact]
    public async Task UpdateUser_ShouldUpdateUserFields_WhenCommandIsValid()
    {
        // Arrange - создаем пользователя
        var userEmail = "update-test@example.com";
        var userName = "Original Name";
        
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
            
            var result = await sender.Send(registerCommand);
            
            // Получаем ID созданного пользователя из БД
            var user = DbContext.Users.FirstOrDefault(u => u.Email == userEmail);
            userId = user.Id;
        }
        
        // Обновляем данные пользователя
        var updatedName = "Updated Name";
        var updatedEmail = "updated-email@example.com";
        var updateParameters = new UpdateUserDto(updatedName, updatedEmail, null);
        
        // Act
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new UpdateUserCommand(userId.ToString(), updateParameters));
        }

        User updatedUser;
        // Assert
        using (var scope = Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<UserServiceDbContext>();
            updatedUser = dbContext.Users.FirstOrDefault(u => u.Id == userId);
        }

        Assert.NotNull(updatedUser);
        Assert.Equal(updatedName, updatedUser.Name);
        Assert.Equal(updatedEmail, updatedUser.Email);
    }
    
    [Fact]
    public async Task UpdateUser_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        var updateParameters = new UpdateUserDto("Some Name", "some@example.com", null);
        var command = new UpdateUserCommand(nonExistentUserId.ToString(), updateParameters);
        
        // Act & Assert
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            await Assert.ThrowsAsync<NotFoundException>(() => 
                sender.Send(command));
        }
    }
    
    [Fact]
    public async Task UpdateUser_ShouldThrowValidationException_WhenUserIdClaimIsEmpty()
    {
        // Arrange
        var userIdClaim = string.Empty;
        var updateParameters = new UpdateUserDto("Some Name", "some@example.com", null);
        var command = new UpdateUserCommand(userIdClaim, updateParameters);
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            Sender.Send(command));
    }
    
    [Fact]
    public async Task UpdateUser_ShouldThrowValidationException_WhenEmailIsInvalid()
    {
        // Arrange - создаем пользователя
        var userEmail = "validation-test@example.com";
        var userName = "Validation Test";
        
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
            
            // Получаем ID созданного пользователя из БД
            var user = DbContext.Users.FirstOrDefault(u => u.Email == userEmail);
            userId = user.Id;
        }
        
        // Обновляем с некорректным email
        var updateParameters = new UpdateUserDto("Valid Name", "invalid-email", null);
        var command = new UpdateUserCommand(userId.ToString(), updateParameters);
        
        // Act & Assert
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            await Assert.ThrowsAsync<ValidationException>(() => 
                sender.Send(command));
        }
    }
    
    [Fact]
    public async Task UpdateUser_ShouldThrowValidationException_WhenNameIsEmpty()
    {
        // Arrange - создаем пользователя
        var userEmail = "name-validation@example.com";
        var userName = "Original Name";
        
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
            
            // Получаем ID созданного пользователя из БД
            var user = DbContext.Users.FirstOrDefault(u => u.Email == userEmail);
            userId = user.Id;
        }
        
        // Обновляем с пустым именем
        var updateParameters = new UpdateUserDto(string.Empty, "valid@example.com", null);
        var command = new UpdateUserCommand(userId.ToString(), updateParameters);
        
        // Act & Assert
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            await Assert.ThrowsAsync<ValidationException>(() => 
                sender.Send(command));
        }
    }
}