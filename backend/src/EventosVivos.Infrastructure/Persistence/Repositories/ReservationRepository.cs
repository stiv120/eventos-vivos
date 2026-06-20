using EventosVivos.Application.Ports;
using EventosVivos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

public sealed class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _context;

    public ReservationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Reservations
            .Include(r => r.Event)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<bool> ReservationCodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        await _context.Reservations.AnyAsync(r => r.ReservationCode == code, cancellationToken);

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default) =>
        await _context.Reservations.AddAsync(reservation, cancellationToken);

    public Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _context.Reservations.Update(reservation);
        return Task.CompletedTask;
    }
}
