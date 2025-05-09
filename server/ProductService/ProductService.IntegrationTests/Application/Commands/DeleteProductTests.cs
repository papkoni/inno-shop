using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.DTOs.Product;
using ProductService.Application.Handlers.Commands.Product.CreateProduct;
using ProductService.Application.Handlers.Commands.Product.DeleteProduct;
using ProductService.Domain.Exceptions;

namespace ProductService.IntegrationTests.Application.Commands;

public class DeleteProductTests: BaseIntegrationTest
{
    public DeleteProductTests(IntegrationTestWebAppFactory factory) 
        : base(factory) { }
    
    [Fact]
    public async Task Delete_ShouldRemoveProduct_WhenProductExists()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var createParameters = new CreateProductDto("Product to Delete", "Will be deleted", 5);
        var createCommand = new CreateProductCommand(createParameters, userId.ToString());
    
        Guid productId;
    
        using (var scope1 = Factory.Services.CreateScope())
        {
            var sender = scope1.ServiceProvider.GetRequiredService<ISender>();
            productId = await sender.Send(createCommand);
        }
    
        // Act
        using (var scope2 = Factory.Services.CreateScope())
        {
            var sender = scope2.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new DeleteProductCommand(productId));
        }
    
        // Assert
        var product = DbContext.Products.FirstOrDefault(p => p.Id == productId); 
        Assert.Null(product);
    }
    
    [Fact]
    public async Task Delete_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        var nonExistentProductId = Guid.NewGuid();
        var deleteCommand = new DeleteProductCommand(nonExistentProductId);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            Sender.Send(deleteCommand));
    }
    
    [Fact]
    public async Task Delete_ShouldRemoveProductFromDatabase_ButNotAffectOtherProducts()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        
        var createParameters1 = new CreateProductDto("Product to Delete", "Will be deleted", 5);
        var createParameters2 = new CreateProductDto("Product to Keep", "Will remain", 10);
        
        var createCommand1 = new CreateProductCommand(createParameters1, userId.ToString());
        var createCommand2 = new CreateProductCommand(createParameters2, userId.ToString());
        
        Guid productId1;
        Guid productId2;
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            productId1 = await sender.Send(createCommand1);
            productId2 = await sender.Send(createCommand2);
        }
        
        // Act
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new DeleteProductCommand(productId1));
        }
        
        // Assert
        var deletedProduct = DbContext.Products.FirstOrDefault(p => p.Id == productId1);
        var remainingProduct = DbContext.Products.FirstOrDefault(p => p.Id == productId2);
        
        Assert.Null(deletedProduct);
        Assert.NotNull(remainingProduct);
        Assert.Equal("Product to Keep", remainingProduct.Title);
    }
    
    [Fact]
    public async Task Delete_ShouldThrowNotFoundException_WhenProductWasAlreadyDeleted()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var createParameters = new CreateProductDto("Product to Delete Twice", "Will be deleted twice", 15);
        var createCommand = new CreateProductCommand(createParameters, userId.ToString());
        
        Guid productId;
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            productId = await sender.Send(createCommand);
        }
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new DeleteProductCommand(productId));
        }
        
        // Act & Assert
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            await Assert.ThrowsAsync<NotFoundException>(() => 
                sender.Send(new DeleteProductCommand(productId)));
        }
    }
    
    [Fact]
    public async Task Delete_ShouldDeleteCorrectProduct_WhenMultipleProductsExist()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        
        var products = new List<Guid>();
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            for (int i = 1; i <= 5; i++)
            {
                var createParams = new CreateProductDto($"Product {i}", $"Description {i}", i * 10);
                var command = new CreateProductCommand(createParams, userId.ToString());
                products.Add(await sender.Send(command));
            }
        }
        
        var productIdToDelete = products[2];
        
        // Act
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new DeleteProductCommand(productIdToDelete));
        }
        
        // Assert
        var deletedProduct = DbContext.Products.FirstOrDefault(p => p.Id == productIdToDelete);
        var remainingProductsCount = DbContext.Products.Count(p => products.Contains(p.Id));
        
        Assert.Null(deletedProduct);
        Assert.Equal(4, remainingProductsCount);
    }
}