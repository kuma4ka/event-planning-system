namespace EventPlanning.Application.DTOs.Guest;

public record GuestDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string CountryCode,
    string PhoneNumber
)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}