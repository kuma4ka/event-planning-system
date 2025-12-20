namespace EventPlanning.Application.DTOs
{
    public record GuestDto(
        string Id,
        string FullName, 
        string Email, 
        string? PhoneNumber
    );
}