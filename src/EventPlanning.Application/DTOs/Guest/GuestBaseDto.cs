namespace EventPlanning.Application.DTOs.Guest;

public abstract record GuestBaseDto(
    Guid EventId,
    string FirstName,
    string LastName,
    string Email,
    string CountryCode,
    string PhoneNumber
);