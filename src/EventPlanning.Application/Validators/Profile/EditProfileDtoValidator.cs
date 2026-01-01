using EventPlanning.Application.Interfaces;
using EventPlanning.Application.DTOs.Profile;
using FluentValidation;

namespace EventPlanning.Application.Validators.Profile;

public class EditProfileDtoValidator : AbstractValidator<EditProfileDto>
{
    public EditProfileDtoValidator(ICountryService countryService)
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50)
            .Matches(@"^\p{Lu}\p{Ll}*$").WithMessage("First name must start with a capital letter.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50)
            .Matches(@"^\p{Lu}\p{Ll}*$").WithMessage("Last name must start with a capital letter.");

        RuleFor(x => x.CountryCode)
            .NotEmpty()
            .Must(code => countryService.GetSupportedCountries().Any(c => c.Code == code))
            .WithMessage("Invalid or unsupported country code.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(15)
            .Matches(@"^\d{7,15}$").WithMessage("Phone number must contain between 7 and 15 digits."); 
    }
}