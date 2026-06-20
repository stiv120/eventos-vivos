using System.Net;
using System.Net.Http.Json;
using EventosVivos.Api.IntegrationTests.Support;
using EventosVivos.Application.DTOs;
using EventosVivos.Domain.Enums;
using FluentAssertions;

namespace EventosVivos.Api.IntegrationTests;

public sealed class EventsApiTests(IntegrationTestWebAppFactory factory) : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private static readonly DateTime BaseUtc = new(2026, 6, 19, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task GetVenues_ShouldReturnSeededReferenceVenues()
    {
        var response = await _client.GetAsync("/api/venues");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var venues = await response.Content.ReadFromJsonAsync<List<VenueResponse>>();
        venues.Should().NotBeNull();
        venues!.Should().HaveCount(3);
        venues.Should().Contain(v => v.Name == "Auditorio Central" && v.Capacity == 200);
    }

    [Fact]
    public async Task CreateEvent_ValidRequest_ShouldReturnCreatedEvent()
    {
        factory.DateTimeProvider.SetUtcNow(BaseUtc);

        var request = BuildValidEventRequest("Integration Conference", venueId: 1, maxCapacity: 150);

        var response = await _client.PostAsJsonAsync("/api/events", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<EventResponse>();
        created.Should().NotBeNull();
        created!.Title.Should().Be("Integration Conference");
        created.AvailableSeats.Should().Be(150);
        created.Status.Should().Be(EventStatus.Active);
    }

    [Fact]
    public async Task CreateEvent_ShortTitle_ShouldReturnBadRequest()
    {
        factory.DateTimeProvider.SetUtcNow(BaseUtc);

        var request = BuildValidEventRequest("ABC", venueId: 1, maxCapacity: 10);

        var response = await _client.PostAsJsonAsync("/api/events", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateEvent_CapacityExceedsVenue_ShouldReturnUnprocessableEntity()
    {
        factory.DateTimeProvider.SetUtcNow(BaseUtc);

        var request = BuildValidEventRequest("Capacity Overflow Event", venueId: 2, maxCapacity: 60);

        var response = await _client.PostAsJsonAsync("/api/events", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        body.Should().Contain("RN-01");
    }

    [Fact]
    public async Task GetEvents_WithTitleFilter_ShouldReturnMatchingEvents()
    {
        factory.DateTimeProvider.SetUtcNow(BaseUtc);

        await _client.PostAsJsonAsync("/api/events", BuildValidEventRequest("Unique Filterable Title", venueId: 3, maxCapacity: 80));

        var response = await _client.GetAsync("/api/events?titleSearch=filterable");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var events = await response.Content.ReadFromJsonAsync<List<EventResponse>>();
        events.Should().NotBeNull();
        events!.Should().ContainSingle(e => e.Title.Contains("Filterable", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetOccupancyReport_ShouldReturnExpectedMetrics()
    {
        factory.DateTimeProvider.SetUtcNow(BaseUtc);

        var eventResponse = await _client.PostAsJsonAsync("/api/events", BuildValidEventRequest("Report Event", venueId: 2, maxCapacity: 50, dayOffset: 25));
        eventResponse.EnsureSuccessStatusCode();
        var createdEvent = await eventResponse.Content.ReadFromJsonAsync<EventResponse>();

        var reservationResponse = await _client.PostAsJsonAsync("/api/reservations", new CreateReservationRequest(
            createdEvent!.Id, 2, "Buyer", "buyer@test.com"));
        reservationResponse.EnsureSuccessStatusCode();

        var reservation = await reservationResponse.Content.ReadFromJsonAsync<ReservationResponse>();

        var confirmResponse = await _client.PostAsync($"/api/reservations/{reservation!.Id}/confirm-payment", null);
        confirmResponse.EnsureSuccessStatusCode();

        var reportResponse = await _client.GetAsync($"/api/events/{createdEvent.Id}/occupancy-report");
        var report = await reportResponse.Content.ReadFromJsonAsync<OccupancyReportResponse>();

        report.Should().NotBeNull();
        report!.TotalSoldTickets.Should().Be(2);
        report.AvailableTickets.Should().Be(48);
        report.TotalRevenue.Should().Be(100m);
        report.OccupancyPercentage.Should().Be(4m);
    }

    private static CreateEventRequest BuildValidEventRequest(
        string title,
        int venueId,
        int maxCapacity,
        int dayOffset = 10) =>
        new(
            title,
            "Valid integration test event description.",
            venueId,
            maxCapacity,
            BaseUtc.AddDays(dayOffset),
            BaseUtc.AddDays(dayOffset).AddHours(3),
            50m,
            EventType.Conference);
}
