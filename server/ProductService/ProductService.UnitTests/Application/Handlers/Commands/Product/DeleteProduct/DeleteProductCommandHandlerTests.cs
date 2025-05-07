using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using ProductService.Application.Handlers.Commands.Product.DeleteProduct;
using ProductService.Application.Interfaces.DB;
using ProductService.Domain.Exceptions;
using Xunit;

namespace ProductService.UnitTests.Application.Handlers.Commands.Product.DeleteProduct
{
    public class DeleteProductCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly DeleteProductCommandHandler _handler;

        public DeleteProductCommandHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.ProductRepository).Returns(_mockProductRepository.Object);
            _handler = new DeleteProductCommandHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_ExistingProduct_DeletesProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existingProduct = new Domain.Entities.Product(
                "Test Product", 
                "Test Description", 
                100m, 
                userId);
            
            var command = new DeleteProductCommand(productId);
            
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);
            
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(Unit.Value, result);
            _mockProductRepository.Verify(repo => repo.Delete(existingProduct), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistingProduct_ThrowsNotFoundException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new DeleteProductCommand(productId);
            
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _handler.Handle(command, CancellationToken.None));
            
            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
            _mockProductRepository.Verify(repo => repo.Delete(It.IsAny<Domain.Entities.Product>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
} 