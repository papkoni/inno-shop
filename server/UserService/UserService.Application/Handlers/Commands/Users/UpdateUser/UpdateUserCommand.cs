using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Handlers.Commands.Users.UpdateUser;

public record UpdateUserCommand(
    string? UserIdClaim, 
    UpdateUserDto UpdateParameters): IRequest<Guid>;