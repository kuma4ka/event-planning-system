namespace EventPlanning.Application.DTOs;

public record CreateGuestDto(int EventId, string FirstName, string LastName, string Email, string? PhoneNumber);