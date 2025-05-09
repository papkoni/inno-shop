using ProductService.Application.DTOs.Product;
using ProductService.Application.Handlers.Commands.Product.CreateProduct;
using ProductService.Domain.Exceptions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace ProductService.IntegrationTests.Application.Commands;

public class CreateProductTests: BaseIntegrationTest
{
    public CreateProductTests(IntegrationTestWebAppFactory factory) 
        : base(factory) { }
    
    [Fact]
    public async Task Create_ShouldAddProduct_WhenCommandIsValid()
    {
        var userIdClaim = new Guid();
        var createParameters = new CreateProductDto("Create Title", "Very good desc", 6);
        var command = new CreateProductCommand(createParameters, userIdClaim.ToString());

        // Act
        var productId = await Sender.Send(command);

        // Assert
        var product = DbContext.Products.FirstOrDefault(p => p.Id == productId);

        Assert.NotNull(product);
    }
    
    [Fact]
    public async Task Create_ShouldThrowUnauthorizedException_WhenUserIdClaimIsNull()
    {
        // Arrange
        var createParameters = new CreateProductDto("Test Title", "Test Description", 10);
        var command = new CreateProductCommand(createParameters, null);
        
        // Act
        Task Action() => Sender.Send(command);
        
        // Assert
        await Assert.ThrowsAsync<UnauthorizedException>(Action);
    }
    
    [Fact]
    public async Task Create_ShouldThrowUnauthorizedException_WhenUserIdClaimIsEmpty()
    {
        // Arrange
        var createParameters = new CreateProductDto("Test Title", "Test Description", 10);
        var command = new CreateProductCommand(createParameters, string.Empty);
        
        // Act
        Task Action() => Sender.Send(command);
        
        // Assert
        await Assert.ThrowsAsync<UnauthorizedException>(Action);
    }
    
    [Fact]
    public async Task Create_ShouldThrowValidationException_WhenTitleIsEmpty()
    {
        // Arrange
        var userIdClaim = Guid.NewGuid();
        var createParameters = new CreateProductDto("", "Test Description", 10);
        var command = new CreateProductCommand(createParameters, userIdClaim.ToString());
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => Sender.Send(command));
    }
    
    [Fact]
    public async Task Create_ShouldThrowValidationException_WhenTitleExceeds100Characters()
    {
        // Arrange
        var userIdClaim = Guid.NewGuid();
        var longTitle = new string('A', 101); // 101 characters
        var createParameters = new CreateProductDto(longTitle, "Test Description", 10);
        var command = new CreateProductCommand(createParameters, userIdClaim.ToString());
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => Sender.Send(command));
    }
    
    [Fact]
    public async Task Create_ShouldThrowValidationException_WhenDescriptionIsEmpty()
    {
        // Arrange
        var userIdClaim = Guid.NewGuid();
        var createParameters = new CreateProductDto("Test Title", "", 10);
        var command = new CreateProductCommand(createParameters, userIdClaim.ToString());
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => Sender.Send(command));
    }
    
    [Fact]
    public async Task Create_ShouldThrowValidationException_WhenDescriptionExceeds1000Characters()
    {
        // Arrange
        var userIdClaim = Guid.NewGuid();
        var longDescription = new string('A', 1001); // 1001 characters
        var createParameters = new CreateProductDto("Test Title", longDescription, 10);
        var command = new CreateProductCommand(createParameters, userIdClaim.ToString());
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => Sender.Send(command));
    }
    
    [Fact]
    public async Task Create_ShouldThrowValidationException_WhenPriceIsZero()
    {
        // Arrange
        var userIdClaim = Guid.NewGuid();
        var createParameters = new CreateProductDto("Test Title", "Test Description", 0);
        var command = new CreateProductCommand(createParameters, userIdClaim.ToString());
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => Sender.Send(command));
    }
    
    [Fact]
    public async Task Create_ShouldThrowValidationException_WhenPriceIsNegative()
    {
        // Arrange
        var userIdClaim = Guid.NewGuid();
        var createParameters = new CreateProductDto("Test Title", "Test Description", -5);
        var command = new CreateProductCommand(createParameters, userIdClaim.ToString());
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => Sender.Send(command));
    }
    
    [Fact]
    public async Task Create_ShouldCreateProductWithCorrectProperties()
    {
        // Arrange
        var userIdClaim = Guid.NewGuid();
        var title = "Test Product";
        var description = "Test Description";
        var price = 15.99m;
        
        var createParameters = new CreateProductDto(title, description, price);
        var command = new CreateProductCommand(createParameters, userIdClaim.ToString());
        
        // Act
        var productId = await Sender.Send(command);
        
        // Assert
        var product = DbContext.Products.FirstOrDefault(p => p.Id == productId);
        
        Assert.NotNull(product);
        Assert.Equal(title, product.Title);
        Assert.Equal(description, product.Description);
        Assert.Equal(price, product.Price);
        Assert.Equal(userIdClaim, product.CreatedByUserId);
        Assert.True(product.IsAvailable);
        Assert.True(DateTime.UtcNow.AddMinutes(-1) <= product.CreatedAt && product.CreatedAt <= DateTime.UtcNow);
    }
}