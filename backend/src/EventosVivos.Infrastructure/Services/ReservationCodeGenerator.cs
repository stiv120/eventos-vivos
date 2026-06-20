using EventosVivos.Application.Ports;

namespace EventosVivos.Infrastructure.Services;

public sealed class ReservationCodeGenerator : IReservationCodeGenerator
{
    private readonly IReservationRepository _reservationRepository;

    public ReservationCodeGenerator(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 10;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var code = $"EV-{Random.Shared.Next(100000, 999999)}";
            if (!await _reservationRepository.ReservationCodeExistsAsync(code, cancellationToken))
            {
                return code;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique reservation code.");
    }
}
