namespace EventPlanning.Application.DTOs;

public record CreateEventDto(
    string Name, 
    string Description, 
    DateTime Date, 
    string Type, 
    int? VenueId,
    bool IsPrivate
);