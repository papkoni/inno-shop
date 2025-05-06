namespace ProductService.Application.DTOs.Product;

public record UpdateProductDto(
    string Title,
    string Description,
    decimal Price,
    bool IsAvailable);