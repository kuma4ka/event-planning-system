using Microsoft.AspNetCore.Identity;

namespace EventPlanning.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    // Additional identity-specific properties can go here if needed.
    // For now, standard IdentityUser is sufficient.
    // Domain-specific profile data (FirstName, LastName) stays in Domain.User.
}
