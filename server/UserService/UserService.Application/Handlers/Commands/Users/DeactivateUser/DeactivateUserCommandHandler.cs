using MediatR;
using UserService.Application.Interfaces.DB;
using UserService.Application.Interfaces.ProductService;
using UserService.Domain.Exceptions;

namespace UserService.Application.Handlers.Commands.Users.DeactivateUser;

public class DeactivateUserCommandHandler: IRequestHandler<DeactivateUserCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductServiceClient _productServiceClient;
    
    public DeactivateUserCommandHandler(
        IUnitOfWork unitOfWork,
        IProductServiceClient productServiceClient)
    {
        _unitOfWork = unitOfWork;
        _productServiceClient = productServiceClient;
    }

    public async Task<Unit> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserIdClaim))
        {
            throw new UnauthorizedException("Unauthorized access");
        }
        
        var existingUser = await _unitOfWork.UserRepository.GetByIdAsync(Guid.Parse(request.UserIdClaim), cancellationToken)
                           ?? throw new NotFoundException($"User with id {request.UserIdClaim} doesn't exists");

        existingUser.IsActive = request.IsActive;
        
        await _productServiceClient.NotifyUserDeactivationAsync(Guid.Parse(request.UserIdClaim), cancellationToken);
        
        _unitOfWork.UserRepository.Update(existingUser);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
       
        return Unit.Value;
    }
}