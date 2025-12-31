namespace EventPlanning.Application.DTOs.Guest;

public record UpdateGuestDto(
    Guid Id,
    Guid EventId,
    string FirstName,
    string LastName,
    string Email,
    string CountryCode,
    string PhoneNumber
) : GuestBaseDto(EventId, FirstName, LastName, Email, CountryCode, PhoneNumber);