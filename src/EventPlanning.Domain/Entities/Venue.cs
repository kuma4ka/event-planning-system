namespace EventPlanning.Domain.Entities;

public sealed class Venue
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    public string Name { get; private set; }
    public string Address { get; private set; }
    public int Capacity { get; private set; }
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }

    public Guid OrganizerId { get; private set; }

    public ICollection<Event> Events { get; private set; } = new List<Event>();


    private Venue()
    {
        Name = null!;
        Address = null!;
        OrganizerId = Guid.Empty;
    }

    public Venue(string name, string address, int capacity, Guid organizerId, string? description = null, string? imageUrl = null)
    {
        if (organizerId == Guid.Empty) throw new ArgumentException("OrganizerId is required.", nameof(organizerId));
        OrganizerId = organizerId;
        
        SetDetails(name, address, capacity, description, imageUrl);
    }

    public void UpdateDetails(string name, string address, int capacity, string? description, string? imageUrl)
    {
        SetDetails(name, address, capacity, description, imageUrl);
    }

    private void SetDetails(string name, string address, int capacity, string? description, string? imageUrl)
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