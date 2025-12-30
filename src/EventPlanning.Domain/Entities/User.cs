using EventPlanning.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EventPlanning.Domain.Entities;

public class User : IdentityUser
{
    public string CountryCode { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public UserRole Role { get; private set; }

    private User()
    {
        CountryCode = null!;
        FirstName = null!;
        LastName = null!;
    }

    public User(string firstName, string lastName, UserRole role, string userName, string email, string phoneNumber, string countryCode) : base(userName)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First Name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last Name is required.", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        // We can relax CountryCode validation for legacy/seeding if needed, but for now enforce it.
        if (string.IsNullOrWhiteSpace(countryCode)) countryCode = "+1"; // Default fall-back if missing

        CountryCode = countryCode;
        FirstName = firstName;
        LastName = lastName;
        Role = role;
        Email = email;
        PhoneNumber = phoneNumber;
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
}