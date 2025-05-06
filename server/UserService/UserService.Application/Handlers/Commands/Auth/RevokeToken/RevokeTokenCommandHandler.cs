using MediatR;
using UserService.Application.Handlers.Commands.Users.DeactivateUser;
using UserService.Application.Interfaces.DB;
using UserService.Domain.Exceptions;

namespace UserService.Application.Handlers.Commands.Auth.RevokeToken;

public class RevokeTokenCommandHandler: IRequestHandler<RevokeTokenCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public RevokeTokenCommandHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _unitOfWork.UserRepository.GetByIdAsync(request.Id, cancellationToken)
                           ?? throw new NotFoundException($"User with id {request.Id} doesn't exists");

        existingUser.RefreshToken.IsRevoked = true;        
        
        _unitOfWork.RefreshTokenRepository.Update(existingUser.RefreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
       
        return Unit.Value;
    }
}