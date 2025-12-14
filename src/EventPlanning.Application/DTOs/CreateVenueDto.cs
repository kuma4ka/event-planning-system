namespace EventPlanning.Application.DTOs;

public record CreateVenueDto(
    string Name,
    string Address,
    int Capacity,
    string? Description,
    string? ImageUrl
);