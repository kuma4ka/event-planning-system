using EventPlanning.Domain.Enums;

namespace EventPlanning.Application.DTOs.Event;

public record EventDto(
    Guid Id,
    string Name,
    string Description,
    DateTime Date,
    EventType Type,
    string OrganizerId,
    string VenueName,
    Guid? VenueId,
    string? VenueImageUrl
);