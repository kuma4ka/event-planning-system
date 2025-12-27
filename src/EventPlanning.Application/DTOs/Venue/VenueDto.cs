namespace EventPlanning.Application.DTOs.Venue;

public record VenueDto(
    int Id,
    string Name,
    string Address,
    int Capacity,
    string? Description = null,
    string? ImageUrl = null
);