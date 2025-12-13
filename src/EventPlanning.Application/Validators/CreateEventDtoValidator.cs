using EventPlanning.Application.DTOs;
using FluentValidation;

namespace EventPlanning.Application.Validators;

public class CreateEventDtoValidator : AbstractValidator<CreateEventDto>
{
    public CreateEventDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Event name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Date)
            .GreaterThan(DateTime.Now).WithMessage("Event date cannot be in the past.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid event type.");
            
        RuleFor(x => x.OrganizerId)
            .NotEmpty().WithMessage("Organizer ID is required.");
    }
}