using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Mapster;
using Moq;
using ProductService.Application.DTOs.Product;
using ProductService.Application.Handlers.Commands.Product.UpdateProduct;
using ProductService.Application.Interfaces.DB;
using ProductService.Domain.Entities;
using ProductService.Domain.Exceptions;
using Xunit;

namespace ProductService.UnitTests.Application.Handlers.Commands.Product.UpdateProduct
{
    public class UpdateProductCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly UpdateProductCommandHandler _handler;
        private readonly TypeAdapterConfig _config;

        public UpdateProductCommandHandlerTests()
        {
            // Создаем отдельный экземпляр конфигурации для тестов
            _config = new TypeAdapterConfig();
            
            // Настройка конфигурации для тестов
            _config.Apply(new MapsterConfigAllTests());
            
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.ProductRepository).Returns(_mockProductRepository.Object);
            _handler = new UpdateProductCommandHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_ExistingProduct_UpdatesAndReturnsId()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existingProduct = new Domain.Entities.Product(
                "Original Title",
                "Original Description",
                50m,
                userId);
            
            // Используем рефлексию для установки Id, так как оно приватное поле
            var idProperty = typeof(Domain.Entities.Product).GetProperty("Id");
            idProperty?.SetValue(existingProduct, productId);
            
            var updateDto = new UpdateProductDto("Updated Title", "Updated Description", 100m, true);
            var command = new UpdateProductCommand(productId, updateDto);
            
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);
            
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // Мануально обновим продукт, чтобы не использовать Mapster
            existingProduct.Title = updateDto.Title;
            existingProduct.Description = updateDto.Description;
            existingProduct.Price = updateDto.Price;
            existingProduct.IsAvailable = updateDto.IsAvailable;

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(productId);
            
            // Verify that the product was updated with new values
            existingProduct.Title.Should().Be("Updated Title");
            existingProduct.Description.Should().Be("Updated Description");
            existingProduct.Price.Should().Be(100m);
            
            _mockProductRepository.Verify(repo => repo.Update(existingProduct), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistingProduct_ThrowsNotFoundException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var updateDto = new UpdateProductDto("Updated Title", "Updated Description", 100m, true);
            var command = new UpdateProductCommand(productId, updateDto);
            
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _handler.Handle(command, CancellationToken.None));
            
            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
            _mockProductRepository.Verify(repo => repo.Update(It.IsAny<Domain.Entities.Product>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    public class MapsterConfigAllTests : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.ForType<UpdateProductDto, Domain.Entities.Product>()
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.IsAvailable, src => src.IsAvailable);
        }
    }
} 