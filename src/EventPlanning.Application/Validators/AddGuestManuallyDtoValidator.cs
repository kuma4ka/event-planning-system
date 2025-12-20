using EventPlanning.Application.DTOs;
using FluentValidation;

namespace EventPlanning.Application.Validators;

public class AddGuestManuallyDtoValidator : AbstractValidator<AddGuestManuallyDto>
{
    public AddGuestManuallyDtoValidator()
    {
        RuleFor(x => x.EventId)
            .GreaterThan(0).WithMessage("Invalid Event ID.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^[\d\+\-\(\)\s]*$").WithMessage("Phone number contains invalid characters.")
            .MaximumLength(20).WithMessage("Phone number is too long.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}