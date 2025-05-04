using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Handlers.Commands.Users.UpdateUser;

public record UpdateUserCommand(
    Guid Id, 
    UpdateUserDto UpdateParameters): IRequest<Guid>;