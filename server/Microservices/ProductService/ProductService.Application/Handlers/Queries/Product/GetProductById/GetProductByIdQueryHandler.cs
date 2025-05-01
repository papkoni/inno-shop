using MapsterMapper;
using MediatR;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces.DB;
using ProductService.Domain.Entities;

namespace ProductService.Application.Handlers.Queries.Product.GetProductById;

public class GetProductByIdQueryHandler: IRequestHandler<GetProductByIdQuery, Domain.Entities.Product>
{ 
    private readonly IUnitOfWork _unitOfWork;
    
    public GetProductByIdQueryHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Domain.Entities.Product> Handle(
        GetProductByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.ProductRepository.GetByIdAsync(request.Id, cancellationToken)
                     ?? throw new NotFoundException($"Product with id '{request.Id}' not found.");

        return product;
    }
}