using System.ComponentModel.DataAnnotations;
using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Handlers.Commands.Users.UserRegistration;

public record UserRegistrationCommand(
    [Required] string Email,
    [Required] string Password,
    [Required] string Name): IRequest<TokensDto>;