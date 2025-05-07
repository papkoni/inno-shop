using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using ProductService.Application.DTOs.Product;
using ProductService.Application.Handlers.Queries.Product.GetFilteredUserProducts;
using ProductService.Application.Interfaces.DB;
using ProductService.Domain.Exceptions;
using Xunit;

namespace ProductService.UnitTests.Application.Handlers.Queries.Product.GetFilteredUserProducts
{
    public class GetFilteredUserProductsQueryHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly GetFilteredUserProductsQueryHandler _handler;

        public GetFilteredUserProductsQueryHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.ProductRepository).Returns(_mockProductRepository.Object);
            _handler = new GetFilteredUserProductsQueryHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_ValidUserId_ReturnsFilteredProducts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userIdStr = userId.ToString();
            var filter = new ProductFilterDto
            {
                Title = "test",
                MinPrice = 50,
                MaxPrice = 200
            };
            var pageNumber = 1;
            var pageSize = 10;
            
            var expectedProducts = new List<Domain.Entities.Product>
            {
                new("Test Product", "Description", 100m, userId)
            };
            
            var query = new GetFilteredUserProductsQuery(userIdStr, filter, pageNumber, pageSize);
            
            _mockProductRepository.Setup(repo => 
                    repo.GetFilteredUserProductsAsync(
                        userId, 
                        filter, 
                        It.IsAny<CancellationToken>(), 
                        pageNumber, 
                        pageSize))
                .ReturnsAsync(expectedProducts);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedProducts);
            _mockProductRepository.Verify(repo => 
                repo.GetFilteredUserProductsAsync(
                    userId, 
                    filter, 
                    It.IsAny<CancellationToken>(), 
                    pageNumber, 
                    pageSize), 
                Times.Once);
        }

        [Fact]
        public async Task Handle_NullUserId_ThrowsUnauthorizedException()
        {
            // Arrange
            var filter = new ProductFilterDto();
            var query = new GetFilteredUserProductsQuery(null, filter, 1, 10);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => 
                _handler.Handle(query, CancellationToken.None));
            
            _mockProductRepository.Verify(repo => 
                repo.GetFilteredUserProductsAsync(
                    It.IsAny<Guid>(), 
                    It.IsAny<ProductFilterDto>(), 
                    It.IsAny<CancellationToken>(), 
                    It.IsAny<int>(), 
                    It.IsAny<int>()), 
                Times.Never);
        }
    }
} 