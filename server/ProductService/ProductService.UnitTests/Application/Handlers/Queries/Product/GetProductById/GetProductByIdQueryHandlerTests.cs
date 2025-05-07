using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using ProductService.Application.Handlers.Queries.Product.GetProductById;
using ProductService.Application.Interfaces.DB;
using ProductService.Domain.Exceptions;
using Xunit;

namespace ProductService.UnitTests.Application.Handlers.Queries.Product.GetProductById
{
    public class GetProductByIdQueryHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly GetProductByIdQueryHandler _handler;

        public GetProductByIdQueryHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.ProductRepository).Returns(_mockProductRepository.Object);
            _handler = new GetProductByIdQueryHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_ExistingProduct_ReturnsProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var existingProduct = new Domain.Entities.Product(
                "Test Product", 
                "Test Description", 
                100m, 
                Guid.NewGuid());
            
            var query = new GetProductByIdQuery(productId);
            
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeSameAs(existingProduct);
            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistingProduct_ThrowsNotFoundException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var query = new GetProductByIdQuery(productId);
            
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _handler.Handle(query, CancellationToken.None));
            
            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
} 