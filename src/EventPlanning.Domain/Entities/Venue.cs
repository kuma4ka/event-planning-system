namespace EventPlanning.Domain.Entities;

public sealed class Venue
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    public string Name { get; private set; }
    public string Address { get; private set; }
    public int Capacity { get; private set; }
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }

    public string OrganizerId { get; private set; }

    public ICollection<Event> Events { get; private set; } = new List<Event>();

    // For EF Core
    private Venue()
    {
        Name = null!;
        Address = null!;
        OrganizerId = null!;
    }

    public Venue(string name, string address, int capacity, string organizerId, string? description = null, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("Address is required.", nameof(address));
        if (capacity < 0) throw new ArgumentException("Capacity cannot be negative.", nameof(capacity));
        if (string.IsNullOrWhiteSpace(organizerId)) throw new ArgumentException("OrganizerId is required.", nameof(organizerId));

        Name = name;
        Address = address;
        Capacity = capacity;
        OrganizerId = organizerId;
        Description = description;
        ImageUrl = imageUrl;
    }

    public void UpdateDetails(string name, string address, int capacity, string? description, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("Address is required.", nameof(address));
        if (capacity < 0) throw new ArgumentException("Capacity cannot be negative.", nameof(capacity));

        Name = name;
        Address = address;
        Capacity = capacity;
        Description = description;
        ImageUrl = imageUrl;
    }
}