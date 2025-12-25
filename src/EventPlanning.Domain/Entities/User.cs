using EventPlanning.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EventPlanning.Domain.Entities;

public class User : IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public UserRole Role { get; set; }
}