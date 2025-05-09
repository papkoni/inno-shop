using FluentAssertions;
using Moq;
using UserService.Application.DTOs;
using UserService.Application.Handlers.Commands.Users.UpdateUser;
using UserService.Application.Interfaces.DB;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using Xunit;

namespace UserService.UnitTests.Application.Commands.Users.UpdateUser;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateUserCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldUpdateUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(
            userId.ToString(),
            new UpdateUserDto
            (
                "Updated Name",
                "updated@example.com",
                null
            )
        );

        var existingUser = new User("Original Name", "hashedPassword", "original@example.com");
        existingUser.Id = userId;

        _unitOfWorkMock
            .Setup(x => x.UserRepository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(userId);
        existingUser.Name.Should().Be(command.UpdateParameters.Name);
        existingUser.Email.Should().Be(command.UpdateParameters.Email);
        
        _unitOfWorkMock.Verify(x => x.UserRepository.Update(existingUser), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(
            userId.ToString(),
            new UpdateUserDto
            (
                "Updated Name",
                "updated@example.com",
                null
            )
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