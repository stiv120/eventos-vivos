namespace EventosVivos.Application.Ports;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
