using EventosVivos.Application.Ports;
using EventosVivos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

public sealed class VenueRepository : IVenueRepository
{
    private readonly AppDbContext _context;

    public VenueRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Venue?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Venues.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<Venue>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Venues.OrderBy(v => v.Name).ToListAsync(cancellationToken);
}
