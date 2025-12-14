namespace EventPlanning.Application.DTOs;

public record UpdateVenueDto(
    int Id,
    string Name,
    string Address,
    int Capacity,
    string? Description,
    string? ImageUrl
);