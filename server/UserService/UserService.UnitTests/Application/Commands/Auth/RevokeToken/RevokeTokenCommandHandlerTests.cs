using FluentAssertions;
using MediatR;
using Moq;
using UserService.Application.Handlers.Commands.Auth.RevokeToken;
using UserService.Application.Interfaces.DB;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using Xunit;

namespace UserService.UnitTests.Application.Commands.Auth.RevokeToken;

public class RevokeTokenCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RevokeTokenCommandHandler _handler;

    public RevokeTokenCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new RevokeTokenCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldRevokeToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new RevokeTokenCommand(userId);
        var existingUser = new User("Test User", "hashedPassword", "test@example.com")
        {
            Id = userId,
            RefreshToken = new RefreshToken("validRefreshToken", 60, userId)
            {
                IsRevoked = false
            }
        };

        _unitOfWorkMock
            .Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _unitOfWorkMock
            .Setup(x => x.RefreshTokenRepository.Update(It.IsAny<RefreshToken>()))
            .Verifiable();
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        existingUser.RefreshToken.IsRevoked.Should().BeTrue();
        
        _unitOfWorkMock.Verify(x => x.RefreshTokenRepository.Update(existingUser.RefreshToken), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new RevokeTokenCommand(userId);

        _unitOfWorkMock
            .Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        _unitOfWorkMock.Verify(x => x.RefreshTokenRepository.Update(It.IsAny<RefreshToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
} 