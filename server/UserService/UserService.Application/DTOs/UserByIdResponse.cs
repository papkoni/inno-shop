using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs;

public record UserByIdResponse(
    [Required] Guid Id,
    [Required] string Email,
    [Required] string Name,
    [Required] string Role,
    [Required] bool IsActive);