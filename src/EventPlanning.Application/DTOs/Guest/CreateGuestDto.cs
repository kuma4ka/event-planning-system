namespace EventPlanning.Application.DTOs.Guest;

public record CreateGuestDto(
    int EventId,
    string FirstName = "",
    string LastName = "",
    string Email = "",
    string? PhoneNumber = null
) : GuestBaseDto(EventId, FirstName, LastName, Email, PhoneNumber);