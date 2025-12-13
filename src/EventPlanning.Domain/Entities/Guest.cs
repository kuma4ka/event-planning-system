namespace EventPlanning.Domain.Entities;

public sealed class Guest
{
    public int Id { get; set; }
    
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }
}