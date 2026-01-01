using EventPlanning.Application.Constants;
using EventPlanning.Domain.Constants;
using EventPlanning.Application.DTOs.Guest;
using FluentValidation;

namespace EventPlanning.Application.Validators.Guest;

public abstract class GuestBaseDtoValidator<T> : AbstractValidator<T> where T : GuestBaseDto
{
    protected GuestBaseDtoValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event ID is required.");

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
            .MaximumLength(254).WithMessage("Email cannot exceed 254 characters.")
            .EmailAddress().WithMessage("Invalid email format.")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$")
            .WithMessage("Email must be a valid address with a domain (e.g., user@example.com).");

        RuleFor(x => x.CountryCode)
            .NotEmpty()
            .Must(code => CountryConstants.SupportedCountries.Any(c => c.Code == code))
            .WithMessage("Invalid or unsupported country code.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(15)
            .Matches(@"^\d{7,15}$").WithMessage("Phone number must contain between 7 and 15 digits (no spaces or symbols).");
    }
}