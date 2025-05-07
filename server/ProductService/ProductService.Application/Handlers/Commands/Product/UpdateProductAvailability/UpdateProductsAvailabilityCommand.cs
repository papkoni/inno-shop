using MediatR;

namespace ProductService.Application.Handlers.Commands.Product.UpdateProductAvailability;

public record UpdateProductsAvailabilityCommand(Guid UserId): IRequest<Unit>;