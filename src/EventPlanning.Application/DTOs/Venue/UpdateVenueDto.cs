using Microsoft.AspNetCore.Http;

namespace EventPlanning.Application.DTOs.Venue;

public record UpdateVenueDto(
    Guid Id,
    string Name,
    string Address,
    int Capacity,
    string? Description,
    string? CurrentImageUrl,
    IFormFile? ImageFile
) : VenueBaseDto(Name, Address, Capacity, Description, ImageFile);