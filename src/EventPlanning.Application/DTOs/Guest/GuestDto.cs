namespace EventPlanning.Application.DTOs.Guest;

public record GuestDto(
    string Id,
    string FullName, 
    string Email, 
    string? PhoneNumber
);