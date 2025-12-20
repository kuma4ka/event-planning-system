using EventPlanning.Domain.Enums;

namespace EventPlanning.Domain.Entities;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsPrivate { get; set; } = false;

    public EventType Type { get; set; }

    public int? VenueId { get; set; }
    public Venue? Venue { get; set; }

    public string OrganizerId { get; set; } = string.Empty;

    public ICollection<Guest> Guests { get; set; } = new List<Guest>();
}