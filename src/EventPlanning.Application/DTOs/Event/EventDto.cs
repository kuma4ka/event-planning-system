using EventPlanning.Domain.Enums;

namespace EventPlanning.Application.DTOs.Event;

public record EventDto(
    int Id,
    string Name,
    string Description,
    DateTime Date,
    EventType Type,
    string OrganizerId,
    string VenueName,
    int? VenueId,
    string? VenueImageUrl
);