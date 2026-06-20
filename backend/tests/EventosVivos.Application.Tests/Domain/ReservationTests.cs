using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Exceptions;
using FluentAssertions;

namespace EventosVivos.Application.Tests.Domain;

public class ReservationTests
{
    private static readonly Venue DefaultVenue = new(1, "Auditorio Central", 200, "Bogotá");
    private static readonly DateTime UtcNow = new(2026, 6, 19, 10, 0, 0, DateTimeKind.Utc);

    private static Event CreateEvent(
        DateTime start,
        decimal price = 50m,
        int capacity = 100)
    {
        return Event.Create(
            "Sample Event Title",
            "Sample event description for testing purposes.",
            DefaultVenue,
            capacity,
            start,
            start.AddHours(3),
            price,
            EventType.Conference,
            UtcNow,
            []);
    }

    [Fact]
    public void Create_ShouldSucceed_WhenRulesAreMet()
    {
        var @event = CreateEvent(UtcNow.AddDays(3));

        var reservation = Reservation.Create(@event, 2, "John Doe", "john@example.com", UtcNow);

        reservation.Status.Should().Be(ReservationStatus.PendingPayment);
        reservation.Quantity.Should().Be(2);
    }

    [Fact]
    public void Create_ShouldThrow_WhenEventStartsInLessThanOneHour()
    {
        var @event = CreateEvent(UtcNow.AddMinutes(30));

        var act = () => Reservation.Create(@event, 1, "Jane Doe", "jane@example.com", UtcNow);

        act.Should().Throw<BusinessRuleException>()
            .Which.RuleCode.Should().Be("RN-04");
    }

    [Fact]
    public void Create_ShouldThrow_WhenQuantityExceedsHighPriceLimit()
    {
        var @event = CreateEvent(UtcNow.AddDays(5), price: 150m);

        var act = () => Reservation.Create(@event, 15, "Buyer", "buyer@example.com", UtcNow);

        act.Should().Throw<BusinessRuleException>()
            .Which.RuleCode.Should().Be("RN-05");
    }

    [Fact]
    public void Create_ShouldThrow_WhenLessThan24HoursAndQuantityExceeds5()
    {
        var @event = CreateEvent(UtcNow.AddHours(20));

        var act = () => Reservation.Create(@event, 6, "Buyer", "buyer@example.com", UtcNow);

        act.Should().Throw<BusinessRuleException>()
            .Which.RuleCode.Should().Be("RN-05");
    }

    [Fact]
    public void ConfirmPayment_ShouldGenerateCode_WhenPending()
    {
        var @event = CreateEvent(UtcNow.AddDays(2));
        var reservation = Reservation.Create(@event, 1, "Buyer", "buyer@example.com", UtcNow);

        reservation.ConfirmPayment("EV-123456");

        reservation.Status.Should().Be(ReservationStatus.Confirmed);
        reservation.ReservationCode.Should().Be("EV-123456");
    }

    [Fact]
    public void ConfirmPayment_ShouldThrow_WhenAlreadyConfirmed()
    {
        var @event = CreateEvent(UtcNow.AddDays(2));
        var reservation = Reservation.Create(@event, 1, "Buyer", "buyer@example.com", UtcNow);
        reservation.ConfirmPayment("EV-111111");

        var act = () => reservation.ConfirmPayment("EV-222222");

        act.Should().Throw<BusinessRuleException>()
            .Which.RuleCode.Should().Be("RESERVATION_CONFIRMED");
    }

    [Fact]
    public void Cancel_ShouldMarkAsLost_WhenConfirmedWithin48Hours()
    {
        var @event = CreateEvent(UtcNow.AddHours(30));
        var reservation = Reservation.Create(@event, 2, "Buyer", "buyer@example.com", UtcNow);
        reservation.ConfirmPayment("EV-333333");

        reservation.Cancel(UtcNow, applyPenalty: true);

        reservation.Status.Should().Be(ReservationStatus.Lost);
        reservation.CancelledAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_ShouldReleaseTickets_WhenConfirmedWithEnoughNotice()
    {
        var @event = CreateEvent(UtcNow.AddDays(5));
        var reservation = Reservation.Create(@event, 2, "Buyer", "buyer@example.com", UtcNow);
        reservation.ConfirmPayment("EV-444444");

        reservation.Cancel(UtcNow, applyPenalty: false);

        reservation.Status.Should().Be(ReservationStatus.Cancelled);
        @event.GetAvailableSeats().Should().Be(100);
    }
}
