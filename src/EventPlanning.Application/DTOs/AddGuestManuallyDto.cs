namespace EventPlanning.Application.DTOs;

public record AddGuestManuallyDto(
    int EventId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber
);