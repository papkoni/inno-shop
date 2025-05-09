using ProductService.Application.DTOs.Product;
using ProductService.Application.Handlers.Commands.Product.CreateProduct;
using ProductService.Application.Handlers.Queries.Product.GetFilteredUserProducts;
using ProductService.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Linq;
using ProductService.Persistence;

namespace ProductService.IntegrationTests.Application.Queries.Product;

public class GetFilteredUserProductsTests: BaseIntegrationTest
{
    public GetFilteredUserProductsTests(IntegrationTestWebAppFactory factory) 
        : base(factory) { }
    
    [Fact]
    public async Task GetFilteredUserProducts_ShouldReturnFilteredProducts_WhenFiltersApplied()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            var create1 = new CreateProductDto("Budget Laptop", "Low cost laptop", 500);
            var create2 = new CreateProductDto("Premium Laptop", "High end laptop", 2000);
            var create3 = new CreateProductDto("Budget Phone", "Low cost phone", 300);
            var create4 = new CreateProductDto("Premium Phone", "High end phone", 1200);
            
            await sender.Send(new CreateProductCommand(create1, userId.ToString()));
            await sender.Send(new CreateProductCommand(create2, userId.ToString()));
            await sender.Send(new CreateProductCommand(create3, userId.ToString()));
            await sender.Send(new CreateProductCommand(create4, userId.ToString()));
        }
        
        var filter = new ProductFilterDto
        {
            Title = "Laptop",
            MaxPrice = 1000
        };
        
        // Act
        List<Domain.Entities.Product> products;
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            products = await sender.Send(new GetFilteredUserProductsQuery(userId.ToString(), filter, 1, 10));
        }
        
        // Assert
        Assert.Single(products);
        Assert.Equal("Budget Laptop", products[0].Title);
        Assert.Equal(500, products[0].Price);
    }
    
    [Fact]
    public async Task GetFilteredUserProducts_ShouldReturnOnlyProductsMatchingPriceRange()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            var create1 = new CreateProductDto("Product 100", "Description", 100);
            var create2 = new CreateProductDto("Product 200", "Description", 200);
            var create3 = new CreateProductDto("Product 300", "Description", 300);
            var create4 = new CreateProductDto("Product 400", "Description", 400);
            var create5 = new CreateProductDto("Product 500", "Description", 500);
            
            await sender.Send(new CreateProductCommand(create1, userId.ToString()));
            await sender.Send(new CreateProductCommand(create2, userId.ToString()));
            await sender.Send(new CreateProductCommand(create3, userId.ToString()));
            await sender.Send(new CreateProductCommand(create4, userId.ToString()));
            await sender.Send(new CreateProductCommand(create5, userId.ToString()));
        }
        
        var filter = new ProductFilterDto
        {
            MinPrice = 200,
            MaxPrice = 400
        };
        
        // Act
        List<Domain.Entities.Product> products;
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            products = await sender.Send(new GetFilteredUserProductsQuery(userId.ToString(), filter, 1, 10));
        }
        
        // Assert
        Assert.Equal(3, products.Count);
        Assert.All(products, p => Assert.InRange(p.Price, 200, 400));
    }
    
    [Fact]
    public async Task GetFilteredUserProducts_ShouldReturnPaginatedResults()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int totalProducts = 10;
        const int pageSize = 3;
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            for (int i = 1; i <= totalProducts; i++)
            {
                var create = new CreateProductDto($"Product {i}", $"Description {i}", i * 100);
                await sender.Send(new CreateProductCommand(create, userId.ToString()));
            }
        }
        
        var filter = new ProductFilterDto();
        
        // Act
        List<Domain.Entities.Product> page1;
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            page1 = await sender.Send(new GetFilteredUserProductsQuery(userId.ToString(), filter, 1, pageSize));
        }
        
        List<Domain.Entities.Product> page2;
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            page2 = await sender.Send(new GetFilteredUserProductsQuery(userId.ToString(), filter, 2, pageSize));
        }
        
        // Assert
        Assert.Equal(pageSize, page1.Count);
        Assert.Equal(pageSize, page2.Count);
        
        var allProductIds = page1.Select(p => p.Id).Concat(page2.Select(p => p.Id)).ToList();
        Assert.Equal(pageSize * 2, allProductIds.Distinct().Count());
    }
    
    [Fact]
    public async Task GetFilteredUserProducts_ShouldReturnEmptyList_WhenNoProductsMatchFilter()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            
            var create1 = new CreateProductDto("Product A", "Description A", 100);
            var create2 = new CreateProductDto("Product B", "Description B", 200);
            
            await sender.Send(new CreateProductCommand(create1, userId.ToString()));
            await sender.Send(new CreateProductCommand(create2, userId.ToString()));
        }
        
        var filter = new ProductFilterDto
        {
            Title = "NonExistent"
        };
        
        // Act
        List<Domain.Entities.Product> products;
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            products = await sender.Send(new GetFilteredUserProductsQuery(userId.ToString(), filter, 1, 10));
        }
        
        // Assert
        Assert.Empty(products);
    }
    
    [Fact]
    public async Task GetFilteredUserProducts_ShouldThrowUnauthorizedException_WhenUserIdIsNull()
    {
        // Arrange
        string? userIdClaim = null;
        var filter = new ProductFilterDto();
        
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            Sender.Send(new GetFilteredUserProductsQuery(userIdClaim, filter, 1, 10)));
    }
    
    [Fact]
    public async Task GetFilteredUserProducts_ShouldThrowUnauthorizedException_WhenUserIdIsEmpty()
    {
        // Arrange
        var userIdClaim = string.Empty;
        var filter = new ProductFilterDto();
        
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            Sender.Send(new GetFilteredUserProductsQuery(userIdClaim, filter, 1, 10)));
    }
    
    [Fact]
    public async Task GetFilteredUserProducts_ShouldReturnOnlyAvailableProducts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductServiceDbContext>();
            
            var create1 = new CreateProductDto("Available Electronics", "Description", 100);
            var create2 = new CreateProductDto("Unavailable Electronics", "Description", 200);
            
            var productId1 = await sender.Send(new CreateProductCommand(create1, userId.ToString()));
            var productId2 = await sender.Send(new CreateProductCommand(create2, userId.ToString()));
            
            var product = dbContext.Products.First(p => p.Id == productId2);
            product.IsAvailable = false;
            await dbContext.SaveChangesAsync();
        }
        
        var filter = new ProductFilterDto
        {
            Title = "Electronics"
        };
        
        // Act
        List<Domain.Entities.Product> products;
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            products = await sender.Send(new GetFilteredUserProductsQuery(userId.ToString(), filter, 1, 10));
        }
        
        // Assert
        Assert.Single(products);
        Assert.Equal("Available Electronics", products[0].Title);
    }
}