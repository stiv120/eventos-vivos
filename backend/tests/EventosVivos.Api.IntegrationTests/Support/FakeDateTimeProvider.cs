using EventosVivos.Application.Ports;

namespace EventosVivos.Api.IntegrationTests.Support;

public sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = new(2026, 6, 19, 10, 0, 0, DateTimeKind.Utc);

    public void SetUtcNow(DateTime utcNow) => UtcNow = utcNow;
}
