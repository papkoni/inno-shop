using Mapster;
using MediatR;
using UserService.Application.DTOs;
using UserService.Application.Interfaces.DB;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;

namespace UserService.Application.Handlers.Queries.Users.GetUserById;

public class GetUserByIdQueryHandler: IRequestHandler<GetUserByIdQuery, UserByIdResponse>
{ 
    private readonly IUnitOfWork _unitOfWork;
    
    public GetUserByIdQueryHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<UserByIdResponse> Handle(
        GetUserByIdQuery request, 
        CancellationToken cancellationToken)
    {
        if (request.UserIdClaim == null)
        {
            throw new UnauthorizedException("Unauthorized access");
        }
        
        var product = await _unitOfWork.UserRepository
                          .GetByIdAsync(
                              Guid.Parse(request.UserIdClaim),
                              cancellationToken)
                      ?? throw new NotFoundException($"Product with id '{request.UserIdClaim}' not found.");

        return product.Adapt<UserByIdResponse>();
    }
}