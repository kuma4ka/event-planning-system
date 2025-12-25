using EventPlanning.Domain.Enums;

namespace EventPlanning.Application.DTOs.Event;

public record EventDetailsDto(
    int Id,
    string Name,
    string Description,
    DateTime Date,
    EventType Type,
    string OrganizerId,
    string VenueName,
    string? VenueImageUrl, 
    int VenueCapacity,     
    bool IsPrivate,
    List<GuestDto> Guests
)
{
    public bool IsOrganizer { get; set; }
    public bool IsJoined { get; set; }

    public bool IsPast => Date < DateTime.Now;

    public int GuestsCount => Guests?.Count ?? 0;

    public int SpotsLeft => VenueCapacity > 0 ? VenueCapacity - GuestsCount : int.MaxValue;

    public bool IsFull => VenueCapacity > 0 && SpotsLeft <= 0;

    public int FillPercent => VenueCapacity > 0 
        ? (int)((double)GuestsCount / VenueCapacity * 100) 
        : 0;
}