using System.Net;
using System.Net.Http.Json;
using EventosVivos.Api.IntegrationTests.Support;
using EventosVivos.Application.DTOs;
using EventosVivos.Domain.Enums;
using FluentAssertions;

namespace EventosVivos.Api.IntegrationTests;

public sealed class BusinessRulesApiTests(IntegrationTestWebAppFactory factory) : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private static readonly DateTime BaseUtc = new(2026, 6, 19, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task RN02_OverlappingEventsAtSameVenue_ShouldBeRejected()
    {
        factory.DateTimeProvider.SetUtcNow(BaseUtc);

        var start = BaseUtc.AddDays(20);
        await CreateEventAsync("Base Overlap Event", venueId: 2, maxCapacity: 20, start, start.AddHours(4));

        var response = await _client.PostAsJsonAsync("/api/events", new CreateEventRequest(
            "Overlapping Event",
            "Second event overlapping schedule.",
            2,
            20,
            start.AddHours(2),
            start.AddHours(6),
            30m,
            EventType.Conference));

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        body.Should().Contain("RN-02");
    }

    [Fact]
    public async Task RN03_WeekendEventAfter22_ShouldBeRejected()
    {
        factory.DateTimeProvider.SetUtcNow(BaseUtc);

        var response = await _client.PostAsJsonAsync("/api/events", new CreateEventRequest(
            "Late Weekend Concert",
            "Weekend event starting after 22:00.",
            3,
            100,
            new DateTime(2026, 6, 20, 22, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 6, 21, 1, 0, 0, DateTimeKind.Utc),
            80m,
            EventType.Concert));

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        body.Should().Contain("RN-03");
    }

    [Fact]
    public async Task RN04_ReservationWithinOneHour_ShouldBeRejected()
    {
        factory.DateTimeProvider.SetUtcNow(BaseUtc);

        var start = BaseUtc.AddMinutes(45);
        var createdEvent = await CreateEventAsync("Soon Event", venueId: 3, maxCapacity: 20, start, start.AddHours(2));

        var response = await _client.PostAsJsonAsync("/api/reservations", new CreateReservationRequest(
            createdEvent.Id, 1, "Late Buyer", "late@test.com"));

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        body.Should().Contain("RN-04");
    }

    [Fact]
    public async Task RN05_HighPriceTicketLimit_ShouldRejectMoreThanTenTickets()
    {
        factory.DateTimeProvider.SetUtcNow(BaseUtc);

        var createdEvent = await CreateEventAsync(
            "Expensive Event",
            venueId: 3,
            maxCapacity: 100,
            BaseUtc.AddDays(30),
            BaseUtc.AddDays(30).AddHours(3),
            ticketPrice: 150m);

        var response = await _client.PostAsJsonAsync("/api/reservations", new CreateReservationRequest(
            createdEvent.Id, 11, "Buyer", "buyer@test.com"));

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        body.Should().Contain("RN-05");
    }

    [Fact]
    public async Task RF03_LessThan24Hours_ShouldLimitToFiveTickets()
    {
        factory.DateTimeProvider.SetUtcNow(BaseUtc);

        var start = BaseUtc.AddHours(20);
        var createdEvent = await CreateEventAsync("Within 24h Event", venueId: 3, maxCapacity: 50, start, start.AddHours(2));

        var response = await _client.PostAsJsonAsync("/api/reservations", new CreateReservationRequest(
            createdEvent.Id, 6, "Buyer", "buyer@test.com"));

        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        body.Should().Contain("RN-05");
    }

    [Fact]
    public async Task RN07_CancelConfirmedWithin48Hours_ShouldMarkReservationAsLost()
    {
        factory.DateTimeProvider.SetUtcNow(BaseUtc);

        var start = BaseUtc.AddHours(30);
        var createdEvent = await CreateEventAsync("Penalty Event", venueId: 1, maxCapacity: 30, start, start.AddHours(2));

        var reservationResponse = await _client.PostAsJsonAsync("/api/reservations", new CreateReservationRequest(
            createdEvent.Id, 2, "Penalty Buyer", "penalty@test.com"));
        var reservation = await reservationResponse.Content.ReadFromJsonAsync<ReservationResponse>();

        await _client.PostAsync($"/api/reservations/{reservation!.Id}/confirm-payment", null);

        var cancelResponse = await _client.PostAsync($"/api/reservations/{reservation.Id}/cancel", null);
        var cancelled = await cancelResponse.Content.ReadFromJsonAsync<ReservationResponse>();

        cancelled!.Status.Should().Be(ReservationStatus.Lost);

        var reportResponse = await _client.GetAsync($"/api/events/{createdEvent.Id}/occupancy-report");
        var report = await reportResponse.Content.ReadFromJsonAsync<OccupancyReportResponse>();

        report!.TotalSoldTickets.Should().Be(0);
        report.AvailableTickets.Should().Be(30);
    }

    private async Task<EventResponse> CreateEventAsync(
        string title,
        int venueId,
        int maxCapacity,
        DateTime start,
        DateTime end,
        decimal ticketPrice = 50m)
    {
        var response = await _client.PostAsJsonAsync("/api/events", new CreateEventRequest(
            title,
            "Business rule integration test event.",
            venueId,
            maxCapacity,
            start,
            end,
            ticketPrice,
            EventType.Conference));

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<EventResponse>())!;
    }
}
