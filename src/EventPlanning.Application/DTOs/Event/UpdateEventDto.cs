using EventPlanning.Domain.Enums;

namespace EventPlanning.Application.DTOs.Event;

public record UpdateEventDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime Date,
    EventType Type,
    Guid VenueId
) : EventBaseDto(Name, Description, Date, Type, VenueId);