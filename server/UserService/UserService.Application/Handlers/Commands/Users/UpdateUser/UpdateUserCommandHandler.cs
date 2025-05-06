using Mapster;
using MapsterMapper;
using MediatR;
using UserService.Application.Interfaces.DB;
using UserService.Domain.Exceptions;

namespace UserService.Application.Handlers.Commands.Users.UpdateUser;

public class UpdateUserCommandHandler: IRequestHandler<UpdateUserCommand,Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Guid> Handle(
        UpdateUserCommand request, 
        CancellationToken cancellationToken)
    {
        if (request.UserIdClaim == null)
        {
            throw new UnauthorizedException("Unauthorized access");
        }
        
        var existingUser = await _unitOfWork.UserRepository
                               .GetByIdAsync(
                                   Guid.Parse(request.UserIdClaim), 
                                   cancellationToken) 
                           ?? throw new NotFoundException($"User with id {request.UserIdClaim} doesn't exists");

        request.UpdateParameters.Adapt(existingUser);
        
        _unitOfWork.UserRepository.Update(existingUser);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return existingUser.Id;
    }
}