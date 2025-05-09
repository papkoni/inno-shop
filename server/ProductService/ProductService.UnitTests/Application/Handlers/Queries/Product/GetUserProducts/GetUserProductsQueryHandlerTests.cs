using FluentAssertions;
using Moq;
using ProductService.Application.Handlers.Queries.Product.GetUserProducts;
using ProductService.Application.Interfaces.DB;
using ProductService.Domain.Exceptions;
using Xunit;

namespace ProductService.UnitTests.Application.Handlers.Queries.Product.GetUserProducts
{
    public class GetUserProductsQueryHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly GetUserProductsQueryHandler _handler;

        public GetUserProductsQueryHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.ProductRepository).Returns(_mockProductRepository.Object);
            _handler = new GetUserProductsQueryHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_ValidUserId_ReturnsUserProducts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userIdStr = userId.ToString();
            var expectedProducts = new List<Domain.Entities.Product>
            {
                new("Product 1", "Description 1", 100m, userId),
                new("Product 2", "Description 2", 200m, userId)
            };
            
            var query = new GetUserProductsQuery(userIdStr);
            
            _mockProductRepository.Setup(repo => 
                    repo.GetUserProductsAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedProducts);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedProducts);
            result.Should().HaveCount(2);
            _mockProductRepository.Verify(repo => 
                repo.GetUserProductsAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NullUserId_ThrowsUnauthorizedException()
        {
            // Arrange
            var query = new GetUserProductsQuery(null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => 
                _handler.Handle(query, CancellationToken.None));
            
            _mockProductRepository.Verify(repo => 
                repo.GetUserProductsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), 
                Times.Never);
        }
    }
} 