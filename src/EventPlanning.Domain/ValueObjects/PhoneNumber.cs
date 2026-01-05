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
        if (!IsValidPhoneNumber(phoneNumber)) throw new ArgumentException("Invalid phone number format.", nameof(phoneNumber));

        return new PhoneNumber(phoneNumber);
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        return phoneNumber.Length >= 7 && phoneNumber.Any(char.IsDigit);
    }

    public override string ToString() => Value;

    public static implicit operator string?(PhoneNumber? phone) => phone?.Value;

    public string Format(string? countryCode = null)
    {
        return Value;
    }
}
