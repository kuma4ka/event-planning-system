namespace EventPlanning.Domain.Entities;

public class Guest
{
    public string Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }

    public string? PhoneNumber { get; private set; }

    public int EventId { get; private set; }
    public Event? Event { get; private set; }

    // For EF Core
    private Guest()
    {
        Id = null!;
        FirstName = null!;
        LastName = null!;
        Email = null!;
    }

    public Guest(string id, string firstName, string lastName, string email, int eventId, string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First Name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last Name is required.", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (eventId <= 0) throw new ArgumentException("EventId is required.", nameof(eventId));

        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        EventId = eventId;
        PhoneNumber = phoneNumber;
    }

    public void UpdateDetails(string firstName, string lastName, string email, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First Name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last Name is required.", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
    }
}