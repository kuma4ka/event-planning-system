using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Services;
using EventPlanning.Application.Validators.Event;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EventPlanning.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateEventDtoValidator>();

        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IVenueService, VenueService>();
        services.AddScoped<IGuestService, GuestService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<INewsletterService, NewsletterService>();

        return services;
    }
}