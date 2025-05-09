using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs.Product;
using ProductService.Application.Handlers.Commands.Product.CreateProduct;
using ProductService.Application.Handlers.Commands.Product.DeleteProduct;
using ProductService.Application.Handlers.Commands.Product.UpdateProduct;
using ProductService.Application.Handlers.Commands.Product.UpdateProductAvailability;
using ProductService.Application.Handlers.Queries.Product.GetFilteredUserProducts;
using ProductService.Application.Handlers.Queries.Product.GetProductById;
using ProductService.Application.Handlers.Queries.Product.GetUserProducts;

namespace ProductService.Presentation.Controllers;

[ApiController]
[Route("products")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var result = await _mediator.Send(new CreateProductCommand(request, userIdClaim), cancellationToken);
        
        return Ok(result);
    }
    
    [Authorize]
    [HttpGet("filtered-products")]
    public async Task<IActionResult> GetFilteredUserProducts(
        [FromQuery] ProductFilterDto filter,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var query = new GetFilteredUserProductsQuery(
            userIdClaim,
            filter,
            pageNumber,
            pageSize
        );
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
    
    [Authorize]
    [HttpGet("user-products")]
    public async Task<IActionResult> GetUserProducts(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var result = await _mediator.Send(new GetUserProductsQuery(userIdClaim), cancellationToken);
        
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        
        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id, 
        [FromBody] UpdateProductDto parametersForUpdate, 
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateProductCommand(
            id, parametersForUpdate),
            cancellationToken);
       
        return Ok(result);
    }
    
    [HttpPatch("availability")]
    public async Task<IActionResult> UpdateAvailability([FromBody] Guid userId, CancellationToken cancellationToken)
    {
        var command = new UpdateProductsAvailabilityCommand(userId);
        await _mediator.Send(command);
        
        return NoContent();
    }
    
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        
        return NoContent();
    }
}
