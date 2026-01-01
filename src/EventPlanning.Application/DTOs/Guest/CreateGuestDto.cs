using EventPlanning.Application.Constants;
using EventPlanning.Domain.Constants;

namespace EventPlanning.Application.DTOs.Guest;

public record CreateGuestDto(
    Guid EventId,
    string FirstName = "",
    string LastName = "",
    string Email = "",
    string CountryCode = CountryConstants.DefaultCode,
    string PhoneNumber = ""
) : GuestBaseDto(EventId, FirstName, LastName, Email, CountryCode, PhoneNumber);