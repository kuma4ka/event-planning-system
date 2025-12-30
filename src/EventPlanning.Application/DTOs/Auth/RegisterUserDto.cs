namespace EventPlanning.Application.DTOs.Auth;

public record RegisterUserDto(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    string CountryCode,
    string PhoneNumber
);