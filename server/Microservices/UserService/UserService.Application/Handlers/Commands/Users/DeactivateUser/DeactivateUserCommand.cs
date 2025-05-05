using MediatR;

namespace UserService.Application.Handlers.Commands.Users.DeactivateUser;

public record DeactivateUserCommand(
    string? UserIdClaim,
    bool IsActive): IRequest<Unit>;