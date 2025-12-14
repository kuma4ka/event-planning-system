using Microsoft.AspNetCore.Http;

namespace EventPlanning.Application.DTOs;

public record UpdateVenueDto(
    int Id,
    string Name,
    string Address,
    int Capacity,
    string? Description,
    string? CurrentImageUrl,
    IFormFile? ImageFile
);