using Mapster;
using MapsterMapper;
using MediatR;
using ProductService.Application.Interfaces.DB;
using ProductService.Domain.Exceptions;

namespace ProductService.Application.Handlers.Commands.Product.UpdateProduct;

public class UpdateProductCommandHandler: IRequestHandler<UpdateProductCommand,Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Guid> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var existingProduct = await _unitOfWork.ProductRepository.GetByIdAsync(request.Id, cancellationToken)
                              ?? throw new NotFoundException($"Product with id {request.Id} doesn't exists");

        request.UpdateParameters.Adapt(existingProduct);
        
        _unitOfWork.ProductRepository.Update(existingProduct);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return existingProduct.Id;
    }
}