using System.Net;
using System.Net.Http.Json;
using EventosVivos.Api.IntegrationTests.Support;
using EventosVivos.Application.DTOs;
using EventosVivos.Domain.Enums;
using FluentAssertions;

namespace EventosVivos.Api.IntegrationTests;

public sealed class ReservationsApiTests(IntegrationTestWebAppFactory factory) : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private static readonly DateTime BaseUtc = new(2026, 6, 19, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task CompleteFlow_CreateReserveConfirmAndCancel_ShouldWork()
    {
        factory.DateTimeProvider.SetUtcNow(BaseUtc);

        var createdEvent = await CreateEventAsync("Flow Event", venueId: 3, maxCapacity: 40, dayOffset: 15);

        var reservationResponse = await _client.PostAsJsonAsync("/api/reservations", new CreateReservationRequest(
            createdEvent.Id, 3, "Stiven Garcia", "stiven@test.com"));

        reservationResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var reservation = await reservationResponse.Content.ReadFromJsonAsync<ReservationResponse>();
        reservation!.Status.Should().Be(ReservationStatus.PendingPayment);

        var confirmResponse = await _client.PostAsync($"/api/reservations/{reservation.Id}/confirm-payment", null);
        var confirmed = await confirmResponse.Content.ReadFromJsonAsync<ReservationResponse>();

        confirmed!.Status.Should().Be(ReservationStatus.Confirmed);
        confirmed.ReservationCode.Should().MatchRegex(@"^EV-\d{6}$");

        var duplicateConfirm = await _client.PostAsync($"/api/reservations/{reservation.Id}/confirm-payment", null);
        duplicateConfirm.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var eventAfterConfirm = await GetEventAsync(createdEvent.Id);
        eventAfterConfirm.AvailableSeats.Should().Be(37);

        var cancelResponse = await _client.PostAsync($"/api/reservations/{reservation.Id}/cancel", null);
        var cancelled = await cancelResponse.Content.ReadFromJsonAsync<ReservationResponse>();

        cancelled!.Status.Should().Be(ReservationStatus.Cancelled);
        cancelled.CancelledAt.Should().NotBeNull();

        var eventAfterCancel = await GetEventAsync(createdEvent.Id);
        eventAfterCancel.AvailableSeats.Should().Be(40);
    }

    [Fact]
    public async Task CreateReservation_InvalidEmail_ShouldReturnBadRequest()
    {
        factory.DateTimeProvider.SetUtcNow(BaseUtc);

        var createdEvent = await CreateEventAsync("Email Validation Event", venueId: 1, maxCapacity: 20, dayOffset: 22);

        var response = await _client.PostAsJsonAsync("/api/reservations", new CreateReservationRequest(
            createdEvent.Id, 1, "Buyer", "invalid-email"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConfirmPayment_CancelledReservation_ShouldReturnUnprocessableEntity()
    {
        factory.DateTimeProvider.SetUtcNow(BaseUtc);

        var createdEvent = await CreateEventAsync("Cancelled Confirm Event", venueId: 2, maxCapacity: 20, dayOffset: 28);

        var reservationResponse = await _client.PostAsJsonAsync("/api/reservations", new CreateReservationRequest(
            createdEvent.Id, 1, "Buyer", "buyer@test.com"));
        var reservation = await reservationResponse.Content.ReadFromJsonAsync<ReservationResponse>();

        await _client.PostAsync($"/api/reservations/{reservation!.Id}/cancel", null);

        var confirmResponse = await _client.PostAsync($"/api/reservations/{reservation.Id}/confirm-payment", null);
        var body = await confirmResponse.Content.ReadAsStringAsync();

        confirmResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        body.Should().Contain("RESERVATION_CANCELLED");
    }

    [Fact]
    public async Task Cancel_AlreadyCancelledReservation_ShouldReturnUnprocessableEntity()
    {
        factory.DateTimeProvider.SetUtcNow(BaseUtc);

        var createdEvent = await CreateEventAsync("Double Cancel Event", venueId: 1, maxCapacity: 20, dayOffset: 10);

        var reservationResponse = await _client.PostAsJsonAsync("/api/reservations", new CreateReservationRequest(
            createdEvent.Id, 1, "Buyer", "buyer@test.com"));
        var reservation = await reservationResponse.Content.ReadFromJsonAsync<ReservationResponse>();

        await _client.PostAsync($"/api/reservations/{reservation!.Id}/cancel", null);

        var secondCancel = await _client.PostAsync($"/api/reservations/{reservation.Id}/cancel", null);
        var body = await secondCancel.Content.ReadAsStringAsync();

        secondCancel.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        body.Should().Contain("RESERVATION_ALREADY_CANCELLED");
    }

    [Fact]
    public async Task ConfirmPayment_WithoutAdminKey_ShouldReturnUnauthorized()
    {
        factory.DateTimeProvider.SetUtcNow(BaseUtc);

        var clientWithoutKey = factory.CreateClient();
        clientWithoutKey.DefaultRequestHeaders.Remove("X-Admin-Key");

        var createdEvent = await CreateEventAsync("Admin Key Event", venueId: 2, maxCapacity: 20, dayOffset: 15);

        var reservationResponse = await clientWithoutKey.PostAsJsonAsync("/api/reservations", new CreateReservationRequest(
            createdEvent.Id, 1, "Buyer", "buyer@test.com"));
        var reservation = await reservationResponse.Content.ReadFromJsonAsync<ReservationResponse>();

        var confirmResponse = await clientWithoutKey.PostAsync(
            $"/api/reservations/{reservation!.Id}/confirm-payment", null);

        confirmResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<EventResponse> CreateEventAsync(string title, int venueId, int maxCapacity, int dayOffset = 15)
    {
        var request = new CreateEventRequest(
            title,
            "Integration test event description.",
            venueId,
            maxCapacity,
            BaseUtc.AddDays(dayOffset),
            BaseUtc.AddDays(dayOffset).AddHours(3),
            75m,
            EventType.Workshop);

        var response = await _client.PostAsJsonAsync("/api/events", request);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<EventResponse>())!;
    }

    private async Task<EventResponse> GetEventAsync(Guid eventId)
    {
        var response = await _client.GetAsync($"/api/events/{eventId}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<EventResponse>())!;
    }
}
