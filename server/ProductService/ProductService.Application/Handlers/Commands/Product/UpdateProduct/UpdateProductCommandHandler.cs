using Mapster;
using MapsterMapper;
using MediatR;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces.DB;

namespace ProductService.Application.Handlers.Commands.Product.UpdateProduct;

public class UpdateProductCommandHandler: IRequestHandler<UpdateProductCommand,Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    public async Task<Guid> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var existingProduct = await _unitOfWork.ProductRepository.GetByIdAsync(request.Id, cancellationToken)
                              ?? throw new NotFoundException($"Budget with id {request.Id} doesn't exists");

        request.ParametersForUpdate.Adapt(existingProduct);
        
        _unitOfWork.ProductRepository.Update(existingProduct);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return existingProduct.Id;
    }
}