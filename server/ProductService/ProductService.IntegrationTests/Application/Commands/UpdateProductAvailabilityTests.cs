using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.DTOs.Product;
using ProductService.Application.Handlers.Commands.Product.CreateProduct;
using ProductService.Application.Handlers.Commands.Product.UpdateProductAvailability;

namespace ProductService.IntegrationTests.Application.Commands;

public class UpdateProductAvailabilityTests: BaseIntegrationTest
{
    public UpdateProductAvailabilityTests(IntegrationTestWebAppFactory factory) 
        : base(factory) { }
    
    [Fact]
    public async Task UpdateAvailability_ShouldSetProductsUnavailable_WhenUserHasProducts()
    {
        var userId = Guid.NewGuid();

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var create1 = new CreateProductDto("First Product", "First Description", 10);
            var create2 = new CreateProductDto("Second Product", "Second Description", 20);

            await sender.Send(new CreateProductCommand(create1, userId.ToString()));
            await sender.Send(new CreateProductCommand(create2, userId.ToString()));
        }

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new UpdateProductsAvailabilityCommand(userId));
        }
        
        var products = DbContext.Products.Where(p => p.CreatedByUserId == userId).ToList();

        Assert.NotEmpty(products);
        Assert.All(products, p => Assert.False(p.IsAvailable));
    }
    
    [Fact]
    public async Task UpdateAvailability_ShouldDoNothing_WhenUserHasNoProducts()
    {
        // Arrange
        var nonExistingUserId = Guid.NewGuid();
        var updateCommand = new UpdateProductsAvailabilityCommand(nonExistingUserId);
        
        // Act
        var result = await Sender.Send(updateCommand);
        
        // Assert
        Assert.Equal(Unit.Value, result);
        var products = DbContext.Products.Where(p => p.CreatedByUserId == nonExistingUserId).ToList();
        Assert.Empty(products);
    }
    
    [Fact]
    public async Task UpdateAvailability_ShouldOnlyAffectProductsOfSpecifiedUser()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var create1 = new CreateProductDto("User1 Product1", "User1 Description1", 10);
            var create2 = new CreateProductDto("User1 Product2", "User1 Description2", 20);
            await sender.Send(new CreateProductCommand(create1, userId1.ToString()));
            await sender.Send(new CreateProductCommand(create2, userId1.ToString()));
            
            var create3 = new CreateProductDto("User2 Product1", "User2 Description1", 30);
            var create4 = new CreateProductDto("User2 Product2", "User2 Description2", 40);
            await sender.Send(new CreateProductCommand(create3, userId2.ToString()));
            await sender.Send(new CreateProductCommand(create4, userId2.ToString()));
        }

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new UpdateProductsAvailabilityCommand(userId1));
        }
        
        var user1Products = DbContext.Products.Where(p => p.CreatedByUserId == userId1).ToList();
        Assert.NotEmpty(user1Products);
        Assert.All(user1Products, p => Assert.False(p.IsAvailable));
        
        var user2Products = DbContext.Products.Where(p => p.CreatedByUserId == userId2).ToList();
        Assert.NotEmpty(user2Products);
        Assert.All(user2Products, p => Assert.True(p.IsAvailable));
    }
    
    [Fact]
    public async Task UpdateAvailability_ShouldHandleMultipleCallsForSameUser()
    {
        // Arrange
        var userId = Guid.NewGuid();

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var create1 = new CreateProductDto("First Product", "First Description", 10);
            var create2 = new CreateProductDto("Second Product", "Second Description", 20);

            await sender.Send(new CreateProductCommand(create1, userId.ToString()));
            await sender.Send(new CreateProductCommand(create2, userId.ToString()));
        }

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new UpdateProductsAvailabilityCommand(userId));
        }
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var create3 = new CreateProductDto("Third Product", "Third Description", 30);
            await sender.Send(new CreateProductCommand(create3, userId.ToString()));
        }
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new UpdateProductsAvailabilityCommand(userId));
        }
        
        var products = DbContext.Products.Where(p => p.CreatedByUserId == userId).ToList();

        Assert.Equal(3, products.Count);
        Assert.All(products, p => Assert.False(p.IsAvailable));
    }
    
    [Fact]
    public async Task UpdateAvailability_ShouldNotAffectOtherProductProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string title = "Test Product";
        const string description = "Test Description";
        const decimal price = 25.99m;

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var create = new CreateProductDto(title, description, price);
            await sender.Send(new CreateProductCommand(create, userId.ToString()));
        }

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new UpdateProductsAvailabilityCommand(userId));
        }
        
        var product = DbContext.Products.FirstOrDefault(p => p.CreatedByUserId == userId);

        Assert.NotNull(product);
        Assert.False(product.IsAvailable); 
        Assert.Equal(title, product.Title); 
        Assert.Equal(description, product.Description); 
        Assert.Equal(price, product.Price); 
    }
}