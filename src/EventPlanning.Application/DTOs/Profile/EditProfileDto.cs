namespace EventPlanning.Application.DTOs.Profile;

public record EditProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    // Код країни (наприклад, +380)
    public string CountryCode { get; set; } = "+380"; 
    
    // Номер абонента (наприклад, 991234567)
    public string? PhoneNumber { get; set; }
    
    public string? Email { get; set; }
    public int OrganizedCount { get; set; }
    public int JoinedCount { get; set; }
}