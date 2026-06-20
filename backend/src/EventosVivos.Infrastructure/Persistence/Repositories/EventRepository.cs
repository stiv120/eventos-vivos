using EventosVivos.Application.Ports;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

public sealed class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public EventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Reservations)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Event>> GetAllAsync(EventFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Reservations)
            .AsQueryable();

        if (filter.Type.HasValue)
        {
            query = query.Where(e => e.Type == filter.Type.Value);
        }

        if (filter.VenueId.HasValue)
        {
            query = query.Where(e => e.VenueId == filter.VenueId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(e => e.Status == filter.Status.Value);
        }

        if (filter.StartDateFrom.HasValue)
        {
            query = query.Where(e => e.StartDateTime >= filter.StartDateFrom.Value);
        }

        if (filter.StartDateTo.HasValue)
        {
            query = query.Where(e => e.StartDateTime <= filter.StartDateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.TitleSearch))
        {
            var search = filter.TitleSearch.Trim().ToLower();
            query = query.Where(e => e.Title.ToLower().Contains(search));
        }

        return await query
            .OrderBy(e => e.StartDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetActiveOverlappingAsync(
        int venueId,
        DateTime start,
        DateTime end,
        Guid? excludeEventId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Events
            .Where(e => e.VenueId == venueId
                && e.Status == EventStatus.Active
                && e.StartDateTime < end
                && e.EndDateTime > start);

        if (excludeEventId.HasValue)
        {
            query = query.Where(e => e.Id != excludeEventId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Event @event, CancellationToken cancellationToken = default) =>
        await _context.Events.AddAsync(@event, cancellationToken);

    public Task UpdateAsync(Event @event, CancellationToken cancellationToken = default)
    {
        _context.Events.Update(@event);
        return Task.CompletedTask;
    }
}
