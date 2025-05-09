using ProductService.Application.DTOs.Product;
using ProductService.Application.Handlers.Commands.Product.CreateProduct;
using ProductService.Application.Handlers.Queries.Product.GetUserProducts;
using ProductService.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Linq;
using ProductService.Persistence;

namespace ProductService.IntegrationTests.Application.Queries.Product;

public class GetUserProductsTests: BaseIntegrationTest
{
    public GetUserProductsTests(IntegrationTestWebAppFactory factory) 
        : base(factory) { }
        
    [Fact]
    public async Task GetUserProducts_ShouldReturnUserProducts_WhenUserHasProducts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            // Создаем несколько продуктов для пользователя
            var create1 = new CreateProductDto("Product 1", "Description 1", 10);
            var create2 = new CreateProductDto("Product 2", "Description 2", 20);
            
            await sender.Send(new CreateProductCommand(create1, userId.ToString()));
            await sender.Send(new CreateProductCommand(create2, userId.ToString()));
        }
        
        // Act
        List<Domain.Entities.Product> products;
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            products = await sender.Send(new GetUserProductsQuery(userId.ToString()));
        }
        
        // Assert
        Assert.NotEmpty(products);
        Assert.Equal(2, products.Count);
        Assert.All(products, p => Assert.Equal(userId, p.CreatedByUserId));
    }
    
    [Fact]
    public async Task GetUserProducts_ShouldReturnEmptyList_WhenUserHasNoProducts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Act
        var products = await Sender.Send(new GetUserProductsQuery(userId.ToString()));
        
        // Assert
        Assert.Empty(products);
    }
    
    [Fact]
    public async Task GetUserProducts_ShouldThrowUnauthorizedException_WhenUserIdIsNull()
    {
        // Arrange
        string? userIdClaim = null;
        
        // Act
        Task Action() => Sender.Send(new GetUserProductsQuery(userIdClaim));
        
        // Assert
        await Assert.ThrowsAsync<UnauthorizedException>(Action);
    }
    
    [Fact]
    public async Task GetUserProducts_ShouldThrowUnauthorizedException_WhenUserIdIsEmpty()
    {
        // Arrange
        var userIdClaim = string.Empty;
        
        // Act
        Task Action() => Sender.Send(new GetUserProductsQuery(userIdClaim));
        
        // Assert
        await Assert.ThrowsAsync<UnauthorizedException>(Action);
    }
    
    [Fact]
    public async Task GetUserProducts_ShouldReturnOnlyAvailableProducts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductServiceDbContext>();
            
            var create1 = new CreateProductDto("Available Product", "Description 1", 10);
            var create2 = new CreateProductDto("Unavailable Product", "Description 2", 20);
            
            var productId1 = await sender.Send(new CreateProductCommand(create1, userId.ToString()));
            var productId2 = await sender.Send(new CreateProductCommand(create2, userId.ToString()));
            
            var product = dbContext.Products.First(p => p.Id == productId2);
            product.IsAvailable = false;
            await dbContext.SaveChangesAsync();
        }
        
        // Act
        List<Domain.Entities.Product> products;
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            products = await sender.Send(new GetUserProductsQuery(userId.ToString()));
        }
        
        // Assert
        Assert.Single(products);
        Assert.Equal("Available Product", products[0].Title);
    }
    
    [Fact]
    public async Task GetUserProducts_ShouldReturnOnlyProductsOfSpecifiedUser()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            // Создаем продукты для первого пользователя
            var create1 = new CreateProductDto("User1 Product", "User1 Description", 10);
            await sender.Send(new CreateProductCommand(create1, userId1.ToString()));
            
            // Создаем продукты для второго пользователя
            var create2 = new CreateProductDto("User2 Product", "User2 Description", 20);
            await sender.Send(new CreateProductCommand(create2, userId2.ToString()));
        }
        
        // Act - запрашиваем продукты первого пользователя
        List<Domain.Entities.Product> products;
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            products = await sender.Send(new GetUserProductsQuery(userId1.ToString()));
        }
        
        // Assert
        Assert.Single(products);
        Assert.Equal(userId1, products[0].CreatedByUserId);
        Assert.Equal("User1 Product", products[0].Title);
    }
}