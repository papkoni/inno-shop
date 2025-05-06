namespace ProductService.Application.DTOs.Product;

public record CreateProductDto( 
    string Title,
    string Description,
    decimal Price);