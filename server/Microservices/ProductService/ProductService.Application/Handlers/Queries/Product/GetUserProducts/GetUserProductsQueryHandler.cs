using MapsterMapper;
using MediatR;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces.DB;
using ProductService.Domain.Entities;

namespace ProductService.Application.Handlers.Queries.Product.GetUserProducts;

public class GetUserProductsQueryHandler: IRequestHandler<GetUserProductsQuery, List<Domain.Entities.Product>>
{ 
    private readonly IUnitOfWork _unitOfWork;
    
    public GetUserProductsQueryHandler(
        IUnitOfWork unitOfWork
    )
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<List<Domain.Entities.Product>> Handle(
        GetUserProductsQuery request, 
        CancellationToken cancellationToken)
    {
        var product = await _unitOfWork
                          .ProductRepository
                          .GetUserProductsAsync(
                              request.CreatedByUserId,
                              cancellationToken); 
        
        return product;
    }
}