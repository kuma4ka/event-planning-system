using EventPlanning.Domain.Enums;

namespace EventPlanning.Application.DTOs;

public record CreateEventDto(
    string Name,
    string Description,
    DateTime Date,
    EventType Type,
    int? VenueId,
    string OrganizerId
);