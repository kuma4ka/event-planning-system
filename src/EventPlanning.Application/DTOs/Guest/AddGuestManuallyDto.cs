namespace EventPlanning.Application.DTOs.Guest;

public record AddGuestManuallyDto(
    Guid EventId,
    string FirstName,
    string LastName,
    string Email,
    string CountryCode,
    string PhoneNumber
) : GuestBaseDto(EventId, FirstName, LastName, Email, CountryCode, PhoneNumber);