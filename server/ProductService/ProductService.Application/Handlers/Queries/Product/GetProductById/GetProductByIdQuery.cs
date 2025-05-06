using MediatR;
using ProductService.Domain.Entities;

namespace ProductService.Application.Handlers.Queries.Product.GetProductById;

public record GetProductByIdQuery(Guid Id): IRequest<Domain.Entities.Product>;