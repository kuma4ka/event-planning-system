using EventPlanning.Application.DTOs.Profile;
using FluentValidation;

namespace EventPlanning.Application.Validators.Profile;

public class EditProfileDtoValidator : AbstractValidator<EditProfileDto>
{
    public EditProfileDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.")
            .Matches(@"^\p{Lu}\p{Ll}*$").WithMessage("First name must start with a capital letter and contain only letters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.")
            .Matches(@"^\p{Lu}\p{Ll}*$").WithMessage("Last name must start with a capital letter and contain only letters.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number is too long.")
            .Matches(@"^\+?[\d\s-]*$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Phone number contains invalid characters.");
    }
}