using EventPlanning.Application.DTOs.Event;
using FluentValidation;

namespace EventPlanning.Application.Validators.Event;

public class UpdateEventDtoValidator : AbstractValidator<UpdateEventDto>
{
    public UpdateEventDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid event ID.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.Date)
            .GreaterThan(DateTime.Now).WithMessage("Event date must be in the future.");
            
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid event type.");

        RuleFor(x => x.VenueId)
            .GreaterThanOrEqualTo(0).WithMessage("Invalid venue identifier.");
    }
}