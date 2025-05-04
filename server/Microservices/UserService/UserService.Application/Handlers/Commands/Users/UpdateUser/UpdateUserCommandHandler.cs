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
    
    public async Task<Guid> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _unitOfWork.UserRepository.GetByIdAsync(request.Id, cancellationToken)
                              ?? throw new NotFoundException($"User with id {request.Id} doesn't exists");

        request.UpdateParameters.Adapt(existingUser);
        
        _unitOfWork.UserRepository.Update(existingUser);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return existingUser.Id;
    }
}