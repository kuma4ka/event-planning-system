using EventPlanning.Domain.Enums;

namespace EventPlanning.Domain.Entities;

public class Event
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public DateTime Date { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public bool IsPrivate { get; private set; } = false;

    public EventType Type { get; private set; }

    public Guid? VenueId { get; private set; }
    public Venue? Venue { get; private set; }

    public string OrganizerId { get; private set; }

    public ICollection<Guest> Guests { get; private set; } = new List<Guest>();

    // Constructor for EF Core
    private Event()
    {
        Name = null!;
        OrganizerId = null!;
    }

    public Event(string name, string? description, DateTime date, EventType type, string organizerId, Guid? venueId = null, bool isPrivate = false)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (date < DateTime.UtcNow) throw new ArgumentException("Date must be in the future.", nameof(date));
        if (string.IsNullOrWhiteSpace(organizerId)) throw new ArgumentException("OrganizerId is required.", nameof(organizerId));

        Name = name;
        Description = description;
        Date = date;
        Type = type;
        OrganizerId = organizerId;
        VenueId = venueId;
        IsPrivate = isPrivate;
    }

    public void AddGuest(Guest guest)
    {
        Guests.Add(guest);
    }

    public void UpdateDetails(string name, string? description, DateTime date, EventType type, Guid? venueId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (date < DateTime.UtcNow) throw new ArgumentException("Cannot move event to the past.", nameof(date));

        Name = name;
        Description = description;
        Date = date;
        Type = type;
        VenueId = venueId;
    }

    public bool IsFull(int currentGuestCount)
    {
        if (Venue == null || Venue.Capacity <= 0) return false;
        return currentGuestCount >= Venue.Capacity;
    }
}