using FluentAssertions;
using Moq;
using UserService.Application.Handlers.Commands.Users.DeactivateUser;
using UserService.Application.Interfaces.DB;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using Xunit;

namespace UserService.UnitTests.Application.Commands.Users.DeactivateUser;

public class DeactivateUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeactivateUserCommandHandler _handler;

    public DeactivateUserCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new DeactivateUserCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldDeactivateUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeactivateUserCommand(
            userId.ToString(),
            false
        );

        var existingUser = new User("Test User", "hashedPassword", "test@example.com")
        {
            Id = userId,
            IsActive = true
        };

        _unitOfWorkMock
            .Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingUser.IsActive.Should().Be(command.IsActive);
        
        _unitOfWorkMock.Verify(x => x.UserRepository.Update(existingUser), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeactivateUserCommand(
            userId.ToString(),
            false
        );

        _unitOfWorkMock
            .Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        _unitOfWorkMock.Verify(x => x.UserRepository.Update(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
} 