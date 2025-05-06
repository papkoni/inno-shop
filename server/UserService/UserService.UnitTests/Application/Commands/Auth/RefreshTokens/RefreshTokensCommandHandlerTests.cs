using FluentAssertions;
using Moq;
using UserService.Application.DTOs;
using UserService.Application.Handlers.Commands.Auth.RefreshTokens;
using UserService.Application.Interfaces.Authentication;
using UserService.Application.Interfaces.DB;
using UserService.Application.Interfaces.Security;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using Xunit;

namespace UserService.UnitTests.Application.Commands.Auth.RefreshTokens;

public class RefreshTokensCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly RefreshTokensCommandHandler _handler;

    public RefreshTokensCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        
        _handler = new RefreshTokensCommandHandler(
            _unitOfWorkMock.Object,
            _jwtProviderMock.Object,
            _passwordHasherMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenTokenIsValid_ShouldReturnNewTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new RefreshTokensCommand("validRefreshToken");
        var existingUser = new User("Test User", "hashedPassword", "test@example.com")
        {
            Id = userId,
            RefreshToken = new RefreshToken("validRefreshToken", 60, userId)
            {
                ExpiryDate = DateTime.UtcNow.AddMinutes(30),
                IsRevoked = false
            }
        };
        var newTokens = new TokensDto("newAccessToken", "newRefreshToken");

        _jwtProviderMock
            .Setup(x => x.ValidateRefreshToken(command.RefreshToken))
            .Returns(true);

        _jwtProviderMock
            .Setup(x => x.GetUserIdFromRefreshToken(command.RefreshToken))
            .Returns(userId);

        _unitOfWorkMock
            .Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _jwtProviderMock
            .Setup(x => x.GenerateTokens(existingUser))
            .Returns(newTokens);

        _jwtProviderMock
            .Setup(x => x.GetRefreshTokenExpirationMinutes())
            .Returns(60);

        _unitOfWorkMock
            .Setup(x => x.RefreshTokenRepository.Update(It.IsAny<RefreshToken>()))
            .Verifiable();
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(newTokens);
        existingUser.RefreshToken.Token.Should().Be(newTokens.RefreshToken);
        existingUser.RefreshToken.IsRevoked.Should().BeFalse();
        
        _unitOfWorkMock.Verify(x => x.RefreshTokenRepository.Update(existingUser.RefreshToken), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTokenIsInvalid_ShouldThrowInvalidTokenException()
    {
        // Arrange
        var command = new RefreshTokensCommand("invalidRefreshToken");

        _jwtProviderMock
            .Setup(x => x.ValidateRefreshToken(command.RefreshToken))
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidTokenException>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        _unitOfWorkMock.Verify(x => x.RefreshTokenRepository.Update(It.IsAny<RefreshToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new RefreshTokensCommand("validRefreshToken");

        _jwtProviderMock
            .Setup(x => x.ValidateRefreshToken(command.RefreshToken))
            .Returns(true);

        _jwtProviderMock
            .Setup(x => x.GetUserIdFromRefreshToken(command.RefreshToken))
            .Returns(userId);

        _unitOfWorkMock
            .Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        _unitOfWorkMock.Verify(x => x.RefreshTokenRepository.Update(It.IsAny<RefreshToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTokenIsExpired_ShouldThrowInvalidTokenException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new RefreshTokensCommand("expiredRefreshToken");
        var existingUser = new User("Test User", "hashedPassword", "test@example.com")
        {
            Id = userId,
            RefreshToken = new RefreshToken("expiredRefreshToken", 60, userId)
            {
                ExpiryDate = DateTime.UtcNow.AddMinutes(-30),
                IsRevoked = false
            }
        };

        _jwtProviderMock
            .Setup(x => x.ValidateRefreshToken(command.RefreshToken))
            .Returns(true);

        _jwtProviderMock
            .Setup(x => x.GetUserIdFromRefreshToken(command.RefreshToken))
            .Returns(userId);

        _unitOfWorkMock
            .Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidTokenException>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        _unitOfWorkMock.Verify(x => x.RefreshTokenRepository.Update(It.IsAny<RefreshToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
} 