using FluentAssertions;
using Moq;
using UserService.Application.DTOs;
using UserService.Application.Handlers.Commands.Users.UserRegistration;
using UserService.Application.Interfaces.Authentication;
using UserService.Application.Interfaces.DB;
using UserService.Application.Interfaces.Security;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using Xunit;

namespace UserService.UnitTests.Application.Commands.Users.UserRegistration;

public class UserRegistrationCommandHandlerTests
{
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UserRegistrationCommandHandler _handler;

    public UserRegistrationCommandHandlerTests()
    {
        _jwtProviderMock = new Mock<IJwtProvider>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        
        _handler = new UserRegistrationCommandHandler(
            _passwordHasherMock.Object,
            _jwtProviderMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldCreateNewUser()
    {
        // Arrange
        var command = new UserRegistrationCommand(
            "test@example.com",
            "password123",
            "Test User"
        );

        var hashedPassword = "hashedPassword123";
        var tokens = new TokensDto("accessToken", "refreshToken");
        var userId = Guid.NewGuid();

        _unitOfWorkMock
            .Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(x => x.Generate(command.Password))
            .Returns(hashedPassword);

        _jwtProviderMock
            .Setup(x => x.GenerateTokens(It.IsAny<User>()))
            .Returns(tokens);

        _jwtProviderMock
            .Setup(x => x.GetRefreshTokenExpirationMinutes())
            .Returns(60);

        _unitOfWorkMock
            .Setup(x => x.UserRepository.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => user.Id = userId);

        _unitOfWorkMock
            .Setup(x => x.RefreshTokenRepository.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()));
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(tokens);
        
        _unitOfWorkMock.Verify(x => x.UserRepository.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.RefreshTokenRepository.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldThrowBadRequestException()
    {
        // Arrange
        var command = new UserRegistrationCommand(
            "test@example.com",
            "password123",
            "Test User"
        );

        var existingUser = new User("Test User", "hashedPassword", "test@example.com");

        _unitOfWorkMock
            .Setup(x => x.UserRepository.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        _unitOfWorkMock.Verify(x => x.UserRepository.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.RefreshTokenRepository.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
} 