using EventPlanning.Domain.Enums;

namespace EventPlanning.Application.DTOs;

public record UpdateEventDto(
    int Id,
    string Name,
    string Description,
    DateTime Date,
    EventType Type,
    int? VenueId
);