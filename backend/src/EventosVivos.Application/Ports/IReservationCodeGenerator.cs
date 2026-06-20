namespace EventosVivos.Application.Ports;

public interface IReservationCodeGenerator
{
    Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken = default);
}
