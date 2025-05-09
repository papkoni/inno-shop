using ProductService.Application.DTOs.Product;
using ProductService.Application.Handlers.Commands.Product.CreateProduct;
using ProductService.Application.Handlers.Queries.Product.GetProductById;
using ProductService.Domain.Exceptions;

namespace ProductService.IntegrationTests.Application.Queries.Product;

public class GetProductByIdTests: BaseIntegrationTest
{
    public GetProductByIdTests(IntegrationTestWebAppFactory factory) 
        : base(factory) { }
    
    [Fact]
    public async Task GetById_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var userIdClaim = new Guid();
        var createParameters = new CreateProductDto("Create Title", "Very good desc", 6);
        var command = new CreateProductCommand(createParameters, userIdClaim.ToString());
        
        var productId = await Sender.Send(command);
        var query = new GetProductByIdQuery(productId);
    
        // Act
        var product = await Sender.Send(query);
    
        // Assert
        Assert.NotNull(product);
    }
    
    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenProductExists()
    {
        // Arrange
        var query = new GetProductByIdQuery(new Guid());
    
        // Act
        Task Action() =>  Sender.Send(query);
    
        // Assert
        await Assert.ThrowsAsync<NotFoundException>(Action);
    }
}