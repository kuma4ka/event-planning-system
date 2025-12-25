namespace EventPlanning.Application.DTOs.Guest;

public abstract record GuestBaseDto(
    int EventId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber
);