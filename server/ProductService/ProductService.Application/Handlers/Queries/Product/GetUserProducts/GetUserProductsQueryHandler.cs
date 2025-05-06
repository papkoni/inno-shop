using MediatR;
using ProductService.Application.Interfaces.DB;
using ProductService.Domain.Exceptions;

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
        if (string.IsNullOrEmpty(request.UserIdClaim))
        {
            throw new UnauthorizedException("Unauthorized access");
        }
        
        var product = await _unitOfWork
                          .ProductRepository
                          .GetUserProductsAsync(
                              Guid.Parse(request.UserIdClaim),
                              cancellationToken); 
        
        return product;
    }
}