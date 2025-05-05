using MediatR;
using UserService.Application.Interfaces.DB;
using UserService.Domain.Exceptions;

namespace UserService.Application.Handlers.Commands.Users.DeactivateUser;

public class DeactivateUserCommandHandler: IRequestHandler<DeactivateUserCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateUserCommandHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _unitOfWork.UserRepository.GetByIdAsync(Guid.Parse(request.UserIdClaim), cancellationToken)
                           ?? throw new NotFoundException($"User with id {request.UserIdClaim} doesn't exists");

        existingUser.IsActive = request.IsActive;
        
        //TODO: обращению к сервису продуктов на софтдете продуктов
        
        _unitOfWork.UserRepository.Update(existingUser);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
       
        return Unit.Value;
    }
}