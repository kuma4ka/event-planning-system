using EventPlanning.Domain.Enums;

namespace EventPlanning.Application.DTOs;

public record EventDetailsDto(
    int Id,
    string Name,
    string Description,
    DateTime Date,
    EventType Type,
    string OrganizerId,
    string VenueName,
    List<GuestDto> Guests
);