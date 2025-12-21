using System.ComponentModel.DataAnnotations;

namespace EventPlanning.Application.DTOs;

public class EditProfileDto
{
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }
    
    public string? Email { get; set; }

    public int OrganizedCount { get; set; }
    public int JoinedCount { get; set; }
}