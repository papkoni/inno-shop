using FluentAssertions;
using Moq;
using UserService.Application.DTOs;
using UserService.Application.Handlers.Commands.Auth.Login;
using UserService.Application.Interfaces.Authentication;
using UserService.Application.Interfaces.DB;
using UserService.Application.Interfaces.Security;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using Xunit;

namespace UserService.UnitTests.Application.Commands.Auth.Login;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        
        _handler = new LoginCommandHandler(
            _unitOfWorkMock.Object,
            _jwtProviderMock.Object,
            _passwordHasherMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ShouldReturnTokens()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "password123");
        var existingUser = new User("Test User", "hashedPassword", "test@example.com");
        var tokens = new TokensDto("accessToken", "refreshToken");
        var refreshToken = new RefreshToken("oldToken", 60, existingUser.Id);
        existingUser.RefreshToken = refreshToken;

        _unitOfWorkMock
            .Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _passwordHasherMock
            .Setup(x => x.Verify(command.Password, existingUser.PasswordHash))
            .Returns(true);

        _jwtProviderMock
            .Setup(x => x.GenerateTokens(existingUser))
            .Returns(tokens);

        _jwtProviderMock
            .Setup(x => x.GetRefreshTokenExpirationMinutes())
            .Returns(60);
        
        _unitOfWorkMock
            .Setup(x => x.RefreshTokenRepository.Update(It.IsAny<RefreshToken>()))
            .Verifiable();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(tokens);
        existingUser.RefreshToken.Token.Should().Be(tokens.RefreshToken);
        existingUser.RefreshToken.IsRevoked.Should().BeFalse();
        
        _unitOfWorkMock.Verify(x => x.RefreshTokenRepository.Update(existingUser.RefreshToken), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "password123");

        _unitOfWorkMock
            .Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        _unitOfWorkMock.Verify(x => x.RefreshTokenRepository.Update(It.IsAny<RefreshToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsInvalid_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "wrongPassword");
        var existingUser = new User("Test User", "hashedPassword", "test@example.com");
        var tokens = new TokensDto("accessToken", "refreshToken");
        var refreshToken = new RefreshToken("oldToken", 60, existingUser.Id);
        existingUser.RefreshToken = refreshToken;

        _unitOfWorkMock
            .Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _passwordHasherMock
            .Setup(x => x.Verify(command.Password, existingUser.PasswordHash))
            .Returns(false);

        _jwtProviderMock
            .Setup(x => x.GenerateTokens(existingUser))
            .Returns(tokens);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
} 