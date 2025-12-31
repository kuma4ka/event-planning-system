namespace EventPlanning.Application.DTOs.Venue;

public record VenueDto(
    Guid Id,
    string Name,
    string Address,
    int Capacity,
    string? Description = null,
    string? ImageUrl = null
);