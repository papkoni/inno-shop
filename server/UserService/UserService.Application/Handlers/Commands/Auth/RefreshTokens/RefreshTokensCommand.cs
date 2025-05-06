using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Handlers.Commands.Auth.RefreshTokens;

public record RefreshTokensCommand(string RefreshToken): IRequest<TokensDto>;