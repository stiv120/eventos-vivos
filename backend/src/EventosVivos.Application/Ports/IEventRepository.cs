using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;

namespace EventosVivos.Application.Ports;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetAllAsync(EventFilter filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetActiveOverlappingAsync(
        int venueId,
        DateTime start,
        DateTime end,
        Guid? excludeEventId = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(Event @event, CancellationToken cancellationToken = default);
    Task UpdateAsync(Event @event, CancellationToken cancellationToken = default);
}

public sealed class EventFilter
{
    public EventType? Type { get; init; }
    public DateTime? StartDateFrom { get; init; }
    public DateTime? StartDateTo { get; init; }
    public int? VenueId { get; init; }
    public EventStatus? Status { get; init; }
    public string? TitleSearch { get; init; }
}
