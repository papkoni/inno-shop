using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Handlers.Commands.Auth.Login;

public record LoginCommand(string Email, string Password): IRequest<TokensDto>;