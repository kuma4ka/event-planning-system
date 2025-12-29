using System.Text.RegularExpressions;

namespace EventPlanning.Domain.ValueObjects;

public record EmailAddress
{
    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static EmailAddress Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (!IsValidEmail(email)) throw new ArgumentException("Invalid email format.", nameof(email));

        return new EmailAddress(email);
    }

    private static bool IsValidEmail(string email)
    {
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        return emailRegex.IsMatch(email);
    }

    public override string ToString() => Value;

    public static implicit operator string(EmailAddress email) => email.Value;
}
