namespace UserService.Application.DTOs;

public record TokensDto(
    string RefreshToken,
    string AccessToken
);