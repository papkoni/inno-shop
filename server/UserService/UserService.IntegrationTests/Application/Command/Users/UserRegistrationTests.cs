using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.DTOs;
using UserService.Application.Handlers.Commands.Users.UserRegistration;
using UserService.Domain.Exceptions;
using FluentValidation;
using System.Threading;

namespace UserService.IntegrationTests.Application.Command.Users;

public class UserRegistrationTests: BaseIntegrationTest
{
    public UserRegistrationTests(IntegrationTestWebAppFactory factory) 
        : base(factory) { }
    
    [Fact]
    public async Task UserRegistration_ShouldCreateNewUser_WhenDataIsValid()
    {
        // Arrange
        var command = new UserRegistrationCommand(
            "test@example.com",
            "Password123",
            "Test User"
        );
        
        // Act
        TokensDto result;
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(command);
        }
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        
        // Проверяем, что пользователь был создан в БД
        var user = DbContext.Users.FirstOrDefault(u => u.Email == command.Email);
        Assert.NotNull(user);
        Assert.Equal(command.Name, user.Name);
        Assert.True(user.IsActive);
    }
    
    [Fact]
    public async Task UserRegistration_ShouldThrowBadRequestException_WhenEmailAlreadyExists()
    {
        // Arrange
        var email = "duplicate@example.com";
        
        // Сначала регистрируем пользователя
        var firstCommand = new UserRegistrationCommand(
            email,
            "Password123",
            "First User"
        );
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(firstCommand);
        }
        
        // Пытаемся зарегистрировать пользователя с тем же email
        var secondCommand = new UserRegistrationCommand(
            email,
            "AnotherPassword123",
            "Second User"
        );
        
        // Act & Assert
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            await Assert.ThrowsAsync<BadRequestException>(() => 
                sender.Send(secondCommand));
        }
    }
    
    [Fact]
    public async Task UserRegistration_ShouldThrowValidationException_WhenEmailIsInvalid()
    {
        // Arrange
        var command = new UserRegistrationCommand(
            "invalid-email",
            "Password123",
            "Test User"
        );
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            Sender.Send(command));
    }
    
    [Fact]
    public async Task UserRegistration_ShouldThrowValidationException_WhenPasswordIsTooShort()
    {
        // Arrange
        var command = new UserRegistrationCommand(
            "test@example.com",
            "short",  // Меньше 6 символов
            "Test User"
        );
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            Sender.Send(command));
    }
    
    [Fact]
    public async Task UserRegistration_ShouldThrowValidationException_WhenNameIsEmpty()
    {
        // Arrange
        var command = new UserRegistrationCommand(
            "test@example.com",
            "Password123",
            ""  // Пустое имя
        );
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            Sender.Send(command));
    }
    
    [Fact]
    public async Task UserRegistration_ShouldCreateRefreshToken_WhenRegistrationSucceeds()
    {
        // Arrange
        var email = "refresh@example.com";
        var command = new UserRegistrationCommand(
            email,
            "Password123",
            "Refresh User"
        );
        
        // Act
        TokensDto result;
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            result = await sender.Send(command);
        }
        
        // Assert
        var user = DbContext.Users.FirstOrDefault(u => u.Email == email);
        Assert.NotNull(user);
        
        var refreshToken = DbContext.RefreshTokens.FirstOrDefault(r => r.UserId == user.Id);
        Assert.NotNull(refreshToken);
        Assert.Equal(result.RefreshToken, refreshToken.Token);
        Assert.False(refreshToken.IsRevoked);
        Assert.True(refreshToken.ExpiryDate > DateTime.UtcNow);
    }
}