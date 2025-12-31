using EventPlanning.Application.DTOs.Guest;
using EventPlanning.Domain.Enums;

namespace EventPlanning.Application.DTOs.Event;

public record EventDetailsDto(
    int VenueCapacity,
    bool IsPrivate,
    List<GuestDto> Guests,
    Guid Id,
    string Name,
    string Description,
    DateTime Date,
    EventType Type,
    string OrganizerId,
    string VenueName,
    Guid? VenueId,
    string? VenueImageUrl,
    string? VenueAddress
) : EventDto(Id, Name, Description, Date, Type, OrganizerId, VenueName, VenueId, VenueImageUrl)
{
    public bool IsOrganizer { get; set; }
    public bool IsJoined { get; set; }

    public bool IsPast => Date < DateTime.Now;

    public int GuestsCount => Guests?.Count ?? 0;

    public int SpotsLeft => VenueCapacity > 0 ? VenueCapacity - GuestsCount : int.MaxValue;

    public bool IsFull => VenueCapacity > 0 && SpotsLeft <= 0;

    public int FillPercent
    {
        get
        {
            if (VenueCapacity <= 0) return 0;
            var percent = (double)GuestsCount / VenueCapacity * 100;
            return (int)Math.Min(100, percent);
        }
    }
}