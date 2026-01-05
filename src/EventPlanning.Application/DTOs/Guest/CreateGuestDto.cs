namespace EventPlanning.Application.DTOs.Guest;

public record CreateGuestDto(
    Guid EventId,
    string FirstName = "",
    string LastName = "",
    string Email = "",
    string CountryCode = "+380",
    string PhoneNumber = ""
) : GuestBaseDto(EventId, FirstName, LastName, Email, CountryCode, PhoneNumber);