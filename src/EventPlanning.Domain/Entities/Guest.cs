using EventPlanning.Domain.ValueObjects;

namespace EventPlanning.Domain.Entities;

public class Guest
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public EmailAddress Email { get; private set; }
    public string CountryCode { get; private set; }

    public PhoneNumber? PhoneNumber { get; private set; }

    public Guid EventId { get; private set; }
    public Event? Event { get; private set; }

    // For EF Core
    private Guest()
    {
        FirstName = null!;
        LastName = null!;
        Email = null!;
        CountryCode = null!;
    }

    public Guest(string firstName, string lastName, string email, Guid eventId, string countryCode, string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First Name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last Name is required.", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (eventId == Guid.Empty) throw new ArgumentException("EventId is required.", nameof(eventId));

        FirstName = firstName;
        LastName = lastName;
        Email = EmailAddress.Create(email);
        EventId = eventId;
        CountryCode = string.IsNullOrWhiteSpace(countryCode) ? "+1" : countryCode;
        PhoneNumber = phoneNumber != null ? PhoneNumber.Create(phoneNumber) : null;
    }

    public void UpdateDetails(string firstName, string lastName, string email, string countryCode, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First Name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last Name is required.", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));

        FirstName = firstName;
        LastName = lastName;
        Email = EmailAddress.Create(email);
        CountryCode = string.IsNullOrWhiteSpace(countryCode) ? "+1" : countryCode;
        PhoneNumber = phoneNumber != null ? PhoneNumber.Create(phoneNumber) : null;
    }
}