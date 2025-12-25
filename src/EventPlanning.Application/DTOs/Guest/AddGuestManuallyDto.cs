namespace EventPlanning.Application.DTOs.Guest;

public record AddGuestManuallyDto(
    int EventId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber
) : GuestBaseDto(EventId, FirstName, LastName, Email, PhoneNumber);