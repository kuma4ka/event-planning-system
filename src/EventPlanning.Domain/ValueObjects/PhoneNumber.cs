using System.Text.RegularExpressions;
using EventPlanning.Domain.Constants;

namespace EventPlanning.Domain.ValueObjects;

public record PhoneNumber
{
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) throw new ArgumentException("Phone number cannot be empty.", nameof(phoneNumber));
        // Basic validation: just ensure it's not gibberish. Full validation is complex.
        if (!IsValidPhoneNumber(phoneNumber)) throw new ArgumentException("Invalid phone number format.", nameof(phoneNumber));

        return new PhoneNumber(phoneNumber);
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        // Simple check for digits and optional symbols +, -, (, ), space
        var phoneRegex = new Regex(@"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$", RegexOptions.Compiled);
        // But let's allow simpler validation for now to avoid breaking existing data too much
        return phoneNumber.Length >= 7 && phoneNumber.Any(char.IsDigit);
    }

    public override string ToString() => Value;

    public static implicit operator string?(PhoneNumber? phone) => phone?.Value;

    public static (string CountryCode, string LocalNumber) Parse(string? fullPhoneNumber)
    {
        if (string.IsNullOrEmpty(fullPhoneNumber)) 
            return (CountryConstants.DefaultCode, string.Empty);

        var country = CountryConstants.SupportedCountries
            .OrderByDescending(c => c.Code.Length)
            .FirstOrDefault(c => fullPhoneNumber.StartsWith(c.Code));

        if (country != null)
        {
            var localNumber = fullPhoneNumber.Substring(country.Code.Length);
            return (country.Code, localNumber);
        }

        return (CountryConstants.DefaultCode, fullPhoneNumber);
    }
    
    public string Format(string? countryCode = null)
    {
        // If country code is provided and matches start, we can try to format securely 
        // or just return Value as it is stored fully qualified.
        return Value;
    }
}
