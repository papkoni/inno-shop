using MediatR;

namespace UserService.Application.Handlers.Commands.Auth.RevokeToken;

public record RevokeTokenCommand(Guid Id): IRequest<Unit>;