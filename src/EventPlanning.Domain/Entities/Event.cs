using EventPlanning.Domain.Enums;

namespace EventPlanning.Domain.Entities;

public sealed class Event
{
    public int Id { get; set; }
    
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public EventType Type { get; set; }

    public int? VenueId { get; set; }
    public Venue? Venue { get; set; }

    public required string OrganizerId { get; set; }

    public ICollection<Guest> Guests { get; set; } = new List<Guest>();
}