using MediatR;
using ProductService.Application.Interfaces.DB;
using ProductService.Domain.Exceptions;

namespace ProductService.Application.Handlers.Commands.Product.UpdateProductAvailability;

public class UpdateProductsAvailabilityCommandHandler: IRequestHandler<UpdateProductsAvailabilityCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductsAvailabilityCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateProductsAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.ProductRepository
            .GetUserProductsAsync(
                request.UserId, 
                cancellationToken);

        if (products.Count == 0) 
        {
            return Unit.Value;
        }        
        
        foreach(var product in products)
        {
            product.IsAvailable = false;
            _unitOfWork.ProductRepository.Update(product);
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
         
        return Unit.Value;
    }
}