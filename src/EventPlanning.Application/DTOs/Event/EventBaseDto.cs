using EventPlanning.Domain.Enums;

namespace EventPlanning.Application.DTOs.Event;

public abstract record EventBaseDto(
    string Name,
    string Description,
    DateTime Date,
    EventType Type,
    int VenueId
);