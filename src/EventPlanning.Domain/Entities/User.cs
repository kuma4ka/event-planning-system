using EventPlanning.Domain.Enums;

namespace EventPlanning.Domain.Entities;

public class User
{
    public string Id { get; private set; }
    public string UserName { get; private set; }
    public string Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    
    public string CountryCode { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public UserRole Role { get; private set; }

    private User()
    {
        Id = Guid.NewGuid().ToString();
        UserName = null!;
        Email = null!;
        CountryCode = null!;
        FirstName = null!;
        LastName = null!;
    }

    public User(string id, string firstName, string lastName, UserRole role, string userName, string email, string phoneNumber, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First Name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last Name is required.", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        
        if (string.IsNullOrWhiteSpace(countryCode)) countryCode = "+1";

        Id = id;
        UserName = userName;
        Email = email;
        PhoneNumber = phoneNumber;

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

    public void SetCountryCode(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode)) throw new ArgumentException("Country Code is required.", nameof(countryCode));
        CountryCode = countryCode;
    }

    public void UpdatePhoneNumber(string? phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }
}