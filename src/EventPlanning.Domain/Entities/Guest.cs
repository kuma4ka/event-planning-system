namespace EventPlanning.Domain.Entities;

public class Guest
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }
}