using MediatR;
using ProductService.Application.Interfaces.DB;
using ProductService.Domain.Exceptions;

namespace ProductService.Application.Handlers.Commands.Product.DeleteProduct;

public class DeleteProductCommandHandler: IRequestHandler<DeleteProductCommand,Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    
    public DeleteProductCommandHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var existingProduct = await _unitOfWork.ProductRepository.GetByIdAsync(request.Id, cancellationToken)
                                     ?? throw new NotFoundException($"Product with id {request.Id} doesn't exists");

        _unitOfWork.ProductRepository.Delete(existingProduct);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}