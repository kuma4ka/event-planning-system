using Microsoft.AspNetCore.Http;

namespace EventPlanning.Application.DTOs.Venue;

public abstract record VenueBaseDto(
    string Name,
    string Address,
    int Capacity,
    string? Description,
    IFormFile? ImageFile
);