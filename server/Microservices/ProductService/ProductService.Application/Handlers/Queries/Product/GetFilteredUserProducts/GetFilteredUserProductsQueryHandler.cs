using MediatR;
using ProductService.Application.Interfaces.DB;

namespace ProductService.Application.Handlers.Queries.Product.GetFilteredUserProducts;

public class GetFilteredUserProductsQueryHandler: IRequestHandler<GetFilteredUserProductsQuery, List<Domain.Entities.Product>>
{ 
    private readonly IUnitOfWork _unitOfWork;
    
    public GetFilteredUserProductsQueryHandler(
        IUnitOfWork unitOfWork
    )
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<List<Domain.Entities.Product>> Handle(
        GetFilteredUserProductsQuery request, 
        CancellationToken cancellationToken)
    {
        var product = await _unitOfWork
                          .ProductRepository
                          .GetFilteredUserProductsAsync(
                              request.CreatedByUserId,
                              request.Filter,
                              cancellationToken,
                              request.PageNumber,
                              request.PageSize);
        
        return product;
    }
}