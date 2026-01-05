namespace EventPlanning.Application.DTOs.Profile;

public record EditProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CountryCode { get; set; } = "+380";
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public int OrganizedCount { get; set; }
    public int JoinedCount { get; set; }

    public string Initials
    {
        get
        {
            if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName))
                return "ME";

            return $"{FirstName[0]}{LastName[0]}".ToUpper();
        }
    }
}