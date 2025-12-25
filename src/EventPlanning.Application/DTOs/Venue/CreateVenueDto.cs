using Microsoft.AspNetCore.Http;

namespace EventPlanning.Application.DTOs.Venue;

public record CreateVenueDto(
    string Name,
    string Address,
    int Capacity,
    string? Description,
    IFormFile? ImageFile
) : VenueBaseDto(Name, Address, Capacity, Description, ImageFile);