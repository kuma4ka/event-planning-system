using EventPlanning.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EventPlanning.Domain.Entities;

public class User : IdentityUser
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public UserRole Role { get; private set; }

    private User()
    {
        FirstName = null!;
        LastName = null!;
    }

    public User(string firstName, string lastName, UserRole role, string userName, string email) : base(userName)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First Name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last Name is required.", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));

        FirstName = firstName;
        LastName = lastName;
        Role = role;
        Email = email;
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First Name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last Name is required.", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
    }
}