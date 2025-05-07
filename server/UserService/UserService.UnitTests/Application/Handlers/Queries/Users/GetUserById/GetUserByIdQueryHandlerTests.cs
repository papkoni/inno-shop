using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Mapster;
using Moq;
using UserService.Application.DTOs;
using UserService.Application.Handlers.Queries.Users.GetUserById;
using UserService.Application.Interfaces.DB;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Domain.Exceptions;
using Xunit;

namespace UserService.UnitTests.Application.Handlers.Queries.Users.GetUserById
{
    public class GetUserByIdQueryHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly GetUserByIdQueryHandler _handler;
        private readonly TypeAdapterConfig _config;

        public GetUserByIdQueryHandlerTests()
        {
            // Создаем отдельный экземпляр конфигурации для тестов
            _config = new TypeAdapterConfig();
            _config.ForType<User, UserByIdResponse>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Role, src => src.Role.ToString());
            
            // Регистрируем конфигурацию
            var mappingRegister = new MapsterRegister();
            TypeAdapterConfig.GlobalSettings.Apply(mappingRegister);

            _mockUserRepository = new Mock<IUserRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.UserRepository).Returns(_mockUserRepository.Object);
            _handler = new GetUserByIdQueryHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_ValidUserId_ReturnsUserByIdResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userIdStr = userId.ToString();
            var user = new User("Test User", "hashed_password", "test@example.com", Role.User, true)
            {
                Id = userId
            };
            
            var query = new GetUserByIdQuery(userIdStr);
            
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(userId);
            result.Email.Should().Be("test@example.com");
            result.Name.Should().Be("Test User");
            result.Role.Should().Be(Role.User.ToString());
            
            _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NullUserId_ThrowsUnauthorizedException()
        {
            // Arrange
            var query = new GetUserByIdQuery(null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => 
                _handler.Handle(query, CancellationToken.None));
            
            _mockUserRepository.Verify(repo => 
                repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), 
                Times.Never);
        }
        
        [Fact]
        public async Task Handle_NonExistingUserId_ThrowsNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userIdStr = userId.ToString();
            var query = new GetUserByIdQuery(userIdStr);
            
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _handler.Handle(query, CancellationToken.None));
            
            _mockUserRepository.Verify(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    // Класс для регистрации маппинга
    public class MapsterRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.ForType<User, UserByIdResponse>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Role, src => src.Role.ToString());
        }
    }
} 