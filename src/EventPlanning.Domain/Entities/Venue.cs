namespace EventPlanning.Domain.Entities;

public sealed class Venue
{
    public int Id { get; set; }
    
    public required string Name { get; set; }
    public required string Address { get; set; }
    public int Capacity { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    public required string OrganizerId { get; set; } 

    public ICollection<Event> Events { get; set; } = new List<Event>();
}