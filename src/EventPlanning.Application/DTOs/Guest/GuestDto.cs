namespace EventPlanning.Application.DTOs.Guest;

public record GuestDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string CountryCode,
    string PhoneNumber
)
{
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