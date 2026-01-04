using System.Diagnostics.CodeAnalysis;
using EventPlanning.Domain.Enums;

namespace EventPlanning.Domain.Entities;

public class Event
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public DateTime Date { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public bool IsPrivate { get; private set; }

    public EventType Type { get; private set; }

    public Guid? VenueId { get; private set; }
    public Venue? Venue { get; private set; }

    public Guid OrganizerId { get; private set; }

    public ICollection<Guest> Guests { get; } = new List<Guest>();
    public bool IsDeleted { get; private set; }

    private Event()
    {
        Name = null!;
        OrganizerId = Guid.Empty;
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
    }

    public Event(string name, string? description, DateTime date, EventType type, Guid organizerId, Guid? venueId = null, bool isPrivate = false)
    {
        if (organizerId == Guid.Empty) throw new ArgumentException("OrganizerId is required.", nameof(organizerId));
        OrganizerId = organizerId;
        IsPrivate = isPrivate;
        
        SetDetails(name, description, date, type, venueId);
    }

    public void AddGuest(Guest guest)
    {
        Guests.Add(guest);
    }

    public void UpdateDetails(string name, string? description, DateTime date, EventType type, Guid? venueId)
    {
        SetDetails(name, description, date, type, venueId);
    }

    [MemberNotNull(nameof(Name))]
    private void SetDetails(string name, string? description, DateTime date, EventType type, Guid? venueId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (date < DateTime.UtcNow) throw new ArgumentException("Date/Time cannot be in the past.", nameof(date));

        Name = name;
        Description = description;
        Date = date;
        Type = type;
        VenueId = venueId;
    }

    public bool HasCapacityLimit => Venue != null && Venue.Capacity > 0;

    public bool IsFull(int currentGuestCount)
    {
        if (!HasCapacityLimit) return false;
        return currentGuestCount >= Venue!.Capacity;
    }

    public void CanAddGuest(int currentGuestCount)
    {
        if (Date < DateTime.UtcNow) throw new InvalidOperationException("Cannot add guests to an event that has already ended.");
        if (IsFull(currentGuestCount)) throw new InvalidOperationException("Venue is fully booked.");
    }
}