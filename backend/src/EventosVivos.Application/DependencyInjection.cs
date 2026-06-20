using EventosVivos.Application.DTOs;
using EventosVivos.Application.Services;
using EventosVivos.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EventosVivos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<EventService>();
        services.AddScoped<ReservationService>();
        services.AddScoped<VenueService>();

        services.AddValidatorsFromAssemblyContaining<CreateEventRequestValidator>();

        return services;
    }
}
