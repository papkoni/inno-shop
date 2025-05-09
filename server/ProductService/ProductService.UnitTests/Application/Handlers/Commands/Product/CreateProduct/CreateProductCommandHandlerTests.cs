using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Mapster;
using Moq;
using ProductService.Application.DTOs.Product;
using ProductService.Application.Handlers.Commands.Product.CreateProduct;
using ProductService.Application.Interfaces.DB;
using ProductService.Domain.Entities;
using ProductService.Domain.Exceptions;
using Xunit;

namespace ProductService.UnitTests.Application.Handlers.Commands.Product.CreateProduct
{
    public class CreateProductCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly CreateProductCommandHandler _handler;
        private readonly TypeAdapterConfig _config;

        public CreateProductCommandHandlerTests()
        {
            _config = new TypeAdapterConfig();
            _config.ForType<CreateProductCommand, Domain.Entities.Product>()
                .ConstructUsing(src => new Domain.Entities.Product(
                    src.CreateParameters.Title,
                    src.CreateParameters.Description,
                    src.CreateParameters.Price,
                    Guid.Parse(src.UserIdClaim),
                    true));

            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.ProductRepository).Returns(_mockProductRepository.Object);
            
            _handler = new CreateProductCommandHandler(_mockUnitOfWork.Object);
            
            var handlerType = _handler.GetType();
            var fieldInfo = handlerType.GetField("_unitOfWork", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(_handler, _mockUnitOfWork.Object);
            }
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsProductId()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var dto = new CreateProductDto("Test Product", "Test Description", 100m);
            var command = new CreateProductCommand(dto, userId);
    
            TypeAdapterConfig<CreateProductCommand, Domain.Entities.Product>
                .NewConfig()
                .ConstructUsing(src => new Domain.Entities.Product(src.CreateParameters.Title, src.CreateParameters.Description, src.CreateParameters.Price, Guid.Parse(src.UserIdClaim), true
                ));
            
            TypeAdapterConfig.GlobalSettings.Compile();
    
            _mockProductRepository.Setup(repo => repo.CreateAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
    
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);
    
            // Assert
            result.Should().NotBeEmpty();
            _mockProductRepository.Verify(repo => repo.CreateAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NullUserId_ThrowsUnauthorizedException()
        {
            // Arrange
            var dto = new CreateProductDto("Test Product", "Test Description", 100m);
            var command = new CreateProductCommand(dto, null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => 
                _handler.Handle(command, CancellationToken.None));
            
            _mockProductRepository.Verify(repo => 
                repo.CreateAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()), 
                Times.Never);
        }
    }
} 