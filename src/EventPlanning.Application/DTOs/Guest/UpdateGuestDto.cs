namespace EventPlanning.Application.DTOs.Guest;

public record UpdateGuestDto(
    string Id,
    int EventId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber
) : GuestBaseDto(EventId, FirstName, LastName, Email, PhoneNumber);