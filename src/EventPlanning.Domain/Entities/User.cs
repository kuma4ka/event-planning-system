using EventPlanning.Domain.Enums;
using EventPlanning.Domain.ValueObjects;

namespace EventPlanning.Domain.Entities;

public class User
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string UserName { get; private set; }
    public string Email { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    
    public string CountryCode { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public UserRole Role { get; private set; }

    private User()
    {
        UserName = null!;
        Email = null!;
        CountryCode = null!;
        FirstName = null!;
        LastName = null!;
    }

    public User(Guid id, string firstName, string lastName, UserRole role, string userName, string email, string? phoneNumber, string countryCode)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First Name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last Name is required.", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        
        if (string.IsNullOrWhiteSpace(countryCode)) countryCode = "+1";

        Id = id;
        UserName = userName;
        Email = email;
        PhoneNumber = phoneNumber != null ? PhoneNumber.Create(phoneNumber) : null;

        CountryCode = countryCode;
        FirstName = firstName;
        LastName = lastName;
        Role = role;
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First Name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last Name is required.", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
    }

    public void UpdateContactInfo(string countryCode, string? phoneNumber)
    {
         if (string.IsNullOrWhiteSpace(countryCode)) throw new ArgumentException("Country Code is required.", nameof(countryCode));
         CountryCode = countryCode;
         PhoneNumber = phoneNumber != null ? PhoneNumber.Create(phoneNumber) : null;
    }
}