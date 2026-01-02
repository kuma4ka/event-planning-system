namespace EventPlanning.Application.DTOs.Guest;

public record GuestDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string CountryCode { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}".Trim();

    public string Initials
    {
        get
        {
            if (string.IsNullOrEmpty(FullName)) return "U";
            return FullName.Length > 0 ? FullName.Substring(0, 1).ToUpper() : "U";
        }
    }
}