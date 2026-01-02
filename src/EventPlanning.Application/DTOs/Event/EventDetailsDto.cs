using EventPlanning.Application.DTOs.Guest;
using EventPlanning.Domain.Enums;

namespace EventPlanning.Application.DTOs.Event;

public record EventDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public EventType Type { get; set; }
    public Guid OrganizerId { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public Guid? VenueId { get; set; }
    public string? VenueImageUrl { get; set; }
    public int VenueCapacity { get; set; }
    public bool IsPrivate { get; set; }
    public string? VenueAddress { get; set; }
    public string OrganizerName { get; set; } = "Unknown";
    public string OrganizerEmail { get; set; } = string.Empty;

    public bool IsOrganizer { get; set; }
    public bool IsJoined { get; set; }
    public List<GuestDto> Guests { get; init; } = new();

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