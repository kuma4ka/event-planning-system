namespace EventPlanning.Application.Models;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string Server { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string SenderName { get; set; } = "Stanza Event Planning";
    public string SenderEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
