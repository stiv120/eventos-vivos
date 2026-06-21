using EventosVivos.Application.DTOs;
using EventosVivos.Domain.Entities;

namespace EventosVivos.Application.Mappings;

public static class EventMapper
{
    public static EventResponse ToResponse(Event @event) =>
        new(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.VenueId,
            @event.Venue?.Name ?? string.Empty,
            @event.Venue?.City ?? string.Empty,
            @event.MaxCapacity,
            @event.StartDateTime,
            @event.EndDateTime,
            @event.TicketPrice,
            @event.Type,
            @event.Status,
            @event.GetAvailableSeats());

    public static OccupancyReportResponse ToOccupancyReport(Event @event)
    {
        var soldTickets = @event.GetConfirmedSeats();
        var availableTickets = @event.GetAvailableSeats();

        var occupancy = @event.MaxCapacity == 0
            ? 0
            : Math.Round((decimal)soldTickets / @event.MaxCapacity * 100, 2);

        return new OccupancyReportResponse(
            @event.Id,
            @event.Title,
            soldTickets,
            availableTickets,
            occupancy,
            soldTickets * @event.TicketPrice,
            @event.Status);
    }
}
