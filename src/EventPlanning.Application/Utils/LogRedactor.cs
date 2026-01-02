using System.Text.RegularExpressions;

namespace EventPlanning.Application.Utils;

public static class LogRedactor
{
    public static string RedactEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "[EMPTY]";

        var atIndex = email.IndexOf('@');
        if (atIndex <= 1) return "***@***.com"; // Too short to show prefix

        var prefix = email.Substring(0, Math.Min(3, atIndex));
        return $"{prefix}***{email.Substring(atIndex)}";
    }

    public static string RedactPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return "[EMPTY]";
        if (phoneNumber.Length < 4) return "***";

        return $"{phoneNumber.Substring(0, Math.Min(3, phoneNumber.Length - 4))}****{phoneNumber.Substring(phoneNumber.Length - 2)}";
    }
}
