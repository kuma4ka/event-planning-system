namespace EventPlanning.Application.DTOs.Profile;

public record EditProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public int OrganizedCount { get; set; }
    public int JoinedCount { get; set; }
}