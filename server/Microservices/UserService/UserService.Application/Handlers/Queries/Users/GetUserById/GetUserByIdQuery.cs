using MediatR;
using UserService.Application.DTOs;
using UserService.Domain.Entities;

namespace UserService.Application.Handlers.Queries.Users.GetUserById;

public record GetUserByIdQuery(string? UserIdClaim): IRequest<UserByIdResponse>;