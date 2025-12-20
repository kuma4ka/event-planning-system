using EventPlanning.Application.DTOs;
using EventPlanning.Domain.Enums;
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

        // Оскільки Type у DTO - це string, перевіряємо, чи існує таке ім'я в Enum
        RuleFor(x => x.Type)
            .IsEnumName(typeof(EventType), caseSensitive: false)
            .WithMessage("Invalid event type.");

        // Валідація VenueId (якщо воно вказане, має бути більше 0)
        RuleFor(x => x.VenueId)
            .GreaterThan(0).When(x => x.VenueId.HasValue)
            .WithMessage("Invalid Venue ID.");
    }
}