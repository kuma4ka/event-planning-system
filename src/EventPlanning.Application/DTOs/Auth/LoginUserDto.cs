namespace EventPlanning.Application.DTOs.Auth;

public record LoginUserDto(
    string Email,
    string Password,
    bool RememberMe
);