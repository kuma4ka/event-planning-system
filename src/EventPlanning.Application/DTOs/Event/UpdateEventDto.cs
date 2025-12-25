using EventPlanning.Domain.Enums;

namespace EventPlanning.Application.DTOs.Event;

public record UpdateEventDto(
    int Id,
    string Name,
    string Description,
    DateTime Date,
    EventType Type,
    int VenueId
) : EventBaseDto(Name, Description, Date, Type, VenueId);