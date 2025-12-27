using EventPlanning.Application.DTOs.Guest;
using FluentValidation;

namespace EventPlanning.Application.Validators.Guest;

public abstract class GuestBaseDtoValidator<T> : AbstractValidator<T> where T : GuestBaseDto
{
    protected GuestBaseDtoValidator()
    {
        RuleFor(x => x.EventId)
            .GreaterThan(0).WithMessage("Event ID is required.");

        var nameRegex = @"^\p{Lu}\p{Ll}*(?:[\s-']\p{Lu}\p{Ll}*)*$";
        var nameErrorMessage =
            "Must start with a capital letter. Parts must be separated by space, hyphen, or apostrophe and also start with a capital (e.g., 'Anna-Maria', 'Mc Donald').";

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.")
            .Matches(nameRegex).WithMessage($"First Name: {nameErrorMessage}");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.")
            .Matches(nameRegex).WithMessage($"Last Name: {nameErrorMessage}");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$")
            .WithMessage("Email must be a valid address with a domain (e.g., user@example.com).");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number is too long.")
            .Matches(@"^\+?[\d\s-]*$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Phone number contains invalid characters (allowed: digits, spaces, -, +).");
    }
}