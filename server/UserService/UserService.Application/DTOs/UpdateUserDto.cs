using UserService.Domain.Enums;

namespace UserService.Application.DTOs;

public record UpdateUserDto(
    string? Name,
    string? Email,
    Role? Role);