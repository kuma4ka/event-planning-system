using EventPlanning.Domain.Enums;

namespace EventPlanning.Application.DTOs.Event;

public record EventDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime Date,
    EventType Type,
    Guid OrganizerId,
    string VenueName,
    Guid? VenueId,
    string? VenueImageUrl
);