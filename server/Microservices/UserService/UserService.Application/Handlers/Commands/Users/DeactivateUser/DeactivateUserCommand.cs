using MediatR;

namespace UserService.Application.Handlers.Commands.Users.DeactivateUser;

public record DeactivateUserCommand(Guid Id, bool IsActive): IRequest<Unit>;