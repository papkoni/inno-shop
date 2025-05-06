using MediatR;

namespace ProductService.Application.Handlers.Commands.Product.DeleteProduct;

public record DeleteProductCommand(Guid Id): IRequest<Unit>;