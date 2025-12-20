namespace EventPlanning.Application.DTOs;

public record EventDetailsDto(
    int Id,
    string Name,
    string Description,
    DateTime Date,
    string Type,
    string OrganizerId,
    string VenueName,
    string? VenueImageUrl, 
    int VenueCapacity,     
    bool IsPrivate,
    List<GuestDto> Guests
);