using System.ComponentModel.DataAnnotations;

namespace EventPlanning.Domain.Entities;

public class NewsletterSubscriber
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Double Opt-In
    public bool IsConfirmed { get; set; } = false;
    public string? ConfirmationToken { get; set; }
}
