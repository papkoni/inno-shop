namespace UserService.Application.DTOs;

public record UpdateUserDto(
    string Name,
    string Email);