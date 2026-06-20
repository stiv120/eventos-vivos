using EventosVivos.Application.Ports;
using EventosVivos.Infrastructure.Persistence;
using EventosVivos.Infrastructure.Persistence.Repositories;
using EventosVivos.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventosVivos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string? environmentName = null)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            if (string.Equals(environmentName, "Testing", StringComparison.OrdinalIgnoreCase))
            {
                var databaseName = configuration["Testing:InMemoryDatabaseName"] ?? "EventosVivosTests";
                options.UseInMemoryDatabase(databaseName);
                return;
            }

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IReservationCodeGenerator, ReservationCodeGenerator>();

        return services;
    }
}
