using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using ProductService.Application.Handlers.Commands.Product.UpdateProductAvailability;
using ProductService.Application.Interfaces.DB;
using Xunit;

namespace ProductService.UnitTests.Application.Handlers.Commands.Product.UpdateProductAvailability
{
    public class UpdateProductsAvailabilityCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly UpdateProductsAvailabilityCommandHandler _handler;

        public UpdateProductsAvailabilityCommandHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.ProductRepository).Returns(_mockProductRepository.Object);
            _handler = new UpdateProductsAvailabilityCommandHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_UserWithProducts_UpdatesAvailabilityToFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var products = new List<Domain.Entities.Product>
            {
                new("Product 1", "Description 1", 100m, userId) { IsAvailable = true },
                new("Product 2", "Description 2", 200m, userId) { IsAvailable = true }
            };
            
            var command = new UpdateProductsAvailabilityCommand(userId);
            
            _mockProductRepository.Setup(repo => repo.GetUserProductsAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);
            
            _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(Unit.Value, result);
            
            // Убедимся, что доступность всех продуктов изменена на false
            foreach (var product in products)
            {
                Assert.False(product.IsAvailable);
            }
            
            _mockProductRepository.Verify(repo => repo.Update(It.IsAny<Domain.Entities.Product>()), Times.Exactly(2));
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UserWithNoProducts_ReturnsUnitValueWithoutUpdates()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var products = new List<Domain.Entities.Product>();
            
            var command = new UpdateProductsAvailabilityCommand(userId);
            
            _mockProductRepository.Setup(repo => repo.GetUserProductsAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(Unit.Value, result);
            _mockProductRepository.Verify(repo => repo.Update(It.IsAny<Domain.Entities.Product>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
} 