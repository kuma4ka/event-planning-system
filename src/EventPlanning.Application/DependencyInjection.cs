using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EventPlanning.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Validators.CreateEventDtoValidator>();

        services.AddScoped<IEventService, EventService>();

        return services;
    }
}