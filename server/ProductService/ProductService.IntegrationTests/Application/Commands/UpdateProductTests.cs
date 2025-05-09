using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.DTOs.Product;
using ProductService.Application.Handlers.Commands.Product.CreateProduct;
using ProductService.Application.Handlers.Commands.Product.DeleteProduct;
using ProductService.Application.Handlers.Commands.Product.UpdateProduct;
using ProductService.Domain.Exceptions;
using FluentValidation;

namespace ProductService.IntegrationTests.Application.Commands;

public class UpdateProductTests: BaseIntegrationTest
{
    public UpdateProductTests(IntegrationTestWebAppFactory factory) 
        : base(factory) { }
    
    [Fact]
    public async Task Update_ShouldThrowNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        var nonExistentProductId = Guid.NewGuid();
        var updateParameters = new UpdateProductDto("Updated Title", "Updated Description", 15, true);
        var updateCommand = new UpdateProductCommand(nonExistentProductId, updateParameters);
        
        // Act
        Task Action() => Sender.Send(updateCommand);
        
        // Assert
        await Assert.ThrowsAsync<NotFoundException>(Action);
    }
    
    [Fact]
    public async Task Update_ShouldUpdateProduct_WhenCommandIsValid()
    {
        var userId = Guid.NewGuid();
        Guid productId;

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var createParams = new CreateProductDto("Initial Title", "Initial Description", 10);
            var createCommand = new CreateProductCommand(createParams, userId.ToString());

            productId = await sender.Send(createCommand);
        }

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var updateParams = new UpdateProductDto("Updated Title", "Updated Description", 15, true);
            var updateCommand = new UpdateProductCommand(productId, updateParams);

            var updatedProductId = await sender.Send(updateCommand);
            Assert.Equal(productId, updatedProductId); // optional assertion here
        }
        
        var product = DbContext.Products.FirstOrDefault(p => p.Id == productId);

        Assert.NotNull(product);
        Assert.Equal("Updated Title", product.Title);
        Assert.Equal("Updated Description", product.Description);
        Assert.Equal(15, product.Price);
        Assert.True(product.IsAvailable);
    }
    
    [Fact]
    public async Task Update_ShouldThrowValidationException_WhenTitleIsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        Guid productId;

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var createParams = new CreateProductDto("Initial Title", "Initial Description", 10);
            var createCommand = new CreateProductCommand(createParams, userId.ToString());

            productId = await sender.Send(createCommand);
        }
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var updateParams = new UpdateProductDto("", "Updated Description", 15, true);
            var updateCommand = new UpdateProductCommand(productId, updateParams);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => sender.Send(updateCommand));
        }
    }
    
    [Fact]
    public async Task Update_ShouldThrowValidationException_WhenTitleExceeds100Characters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        Guid productId;

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var createParams = new CreateProductDto("Initial Title", "Initial Description", 10);
            var createCommand = new CreateProductCommand(createParams, userId.ToString());

            productId = await sender.Send(createCommand);
        }
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var longTitle = new string('A', 101); 
            var updateParams = new UpdateProductDto(longTitle, "Updated Description", 15, true);
            var updateCommand = new UpdateProductCommand(productId, updateParams);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => sender.Send(updateCommand));
        }
    }
    
    [Fact]
    public async Task Update_ShouldThrowValidationException_WhenDescriptionIsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        Guid productId;

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var createParams = new CreateProductDto("Initial Title", "Initial Description", 10);
            var createCommand = new CreateProductCommand(createParams, userId.ToString());

            productId = await sender.Send(createCommand);
        }
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var updateParams = new UpdateProductDto("Updated Title", "", 15, true);
            var updateCommand = new UpdateProductCommand(productId, updateParams);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => sender.Send(updateCommand));
        }
    }
    
    [Fact]
    public async Task Update_ShouldThrowValidationException_WhenDescriptionExceeds1000Characters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        Guid productId;

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var createParams = new CreateProductDto("Initial Title", "Initial Description", 10);
            var createCommand = new CreateProductCommand(createParams, userId.ToString());

            productId = await sender.Send(createCommand);
        }
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var longDescription = new string('A', 1001); // 1001 characters
            var updateParams = new UpdateProductDto("Updated Title", longDescription, 15, true);
            var updateCommand = new UpdateProductCommand(productId, updateParams);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => sender.Send(updateCommand));
        }
    }
    
    [Fact]
    public async Task Update_ShouldThrowValidationException_WhenPriceIsNegative()
    {
        // Arrange
        var userId = Guid.NewGuid();
        Guid productId;

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var createParams = new CreateProductDto("Initial Title", "Initial Description", 10);
            var createCommand = new CreateProductCommand(createParams, userId.ToString());

            productId = await sender.Send(createCommand);
        }
        
        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var updateParams = new UpdateProductDto("Updated Title", "Updated Description", -5, true);
            var updateCommand = new UpdateProductCommand(productId, updateParams);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => sender.Send(updateCommand));
        }
    }
    
    [Fact]
    public async Task Update_ShouldUpdateOnlySpecifiedProduct_WhenMultipleProductsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        Guid productId1;
        Guid productId2;

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var createParams1 = new CreateProductDto("First Product", "First Description", 10);
            var createParams2 = new CreateProductDto("Second Product", "Second Description", 20);
            var createCommand1 = new CreateProductCommand(createParams1, userId.ToString());
            var createCommand2 = new CreateProductCommand(createParams2, userId.ToString());

            productId1 = await sender.Send(createCommand1);
            productId2 = await sender.Send(createCommand2);
        }

        using (var scope = Factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var updateParams = new UpdateProductDto("Updated First Product", "Updated First Description", 15, false);
            var updateCommand = new UpdateProductCommand(productId1, updateParams);

            await sender.Send(updateCommand);
        }
        
        var product1 = DbContext.Products.FirstOrDefault(p => p.Id == productId1);
        var product2 = DbContext.Products.FirstOrDefault(p => p.Id == productId2);

        // Assert product1 
        Assert.NotNull(product1);
        Assert.Equal("Updated First Product", product1.Title);
        Assert.Equal("Updated First Description", product1.Description);
        Assert.Equal(15, product1.Price);
        Assert.False(product1.IsAvailable);
        
        // Assert product2 
        Assert.NotNull(product2);
        Assert.Equal("Second Product", product2.Title);
        Assert.Equal("Second Description", product2.Description);
        Assert.Equal(20, product2.Price);
        Assert.True(product2.IsAvailable);
    }
}