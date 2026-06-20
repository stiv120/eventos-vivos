using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Exceptions;
using FluentAssertions;

namespace EventosVivos.Application.Tests.Domain;

public class EventTests
{
    private static readonly Venue DefaultVenue = new(1, "Auditorio Central", 200, "Bogotá");
    private static readonly DateTime UtcNow = new(2026, 6, 19, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_ShouldSucceed_WhenAllRulesAreMet()
    {
        var start = UtcNow.AddDays(7);
        var end = start.AddHours(3);

        var @event = Event.Create(
            "Tech Conference 2026",
            "A great technology conference for developers.",
            DefaultVenue,
            150,
            start,
            end,
            50m,
            EventType.Conference,
            UtcNow,
            []);

        @event.Title.Should().Be("Tech Conference 2026");
        @event.Status.Should().Be(EventStatus.Active);
        @event.MaxCapacity.Should().Be(150);
    }

    [Fact]
    public void Create_ShouldThrow_WhenCapacityExceedsVenue()
    {
        var start = UtcNow.AddDays(7);
        var end = start.AddHours(2);

        var act = () => Event.Create(
            "Over Capacity Event",
            "This event exceeds venue limits.",
            DefaultVenue,
            250,
            start,
            end,
            30m,
            EventType.Concert,
            UtcNow,
            []);

        act.Should().Throw<BusinessRuleException>()
            .Which.RuleCode.Should().Be("RN-01");
    }

    [Fact]
    public void Create_ShouldThrow_WhenWeekendEventStartsAfter22()
    {
        // 2026-06-20 is Saturday
        var start = new DateTime(2026, 6, 20, 22, 30, 0, DateTimeKind.Utc);
        var end = start.AddHours(2);

        var act = () => Event.Create(
            "Late Night Concert",
            "A concert starting too late on weekend.",
            DefaultVenue,
            100,
            start,
            end,
            80m,
            EventType.Concert,
            UtcNow,
            []);

        act.Should().Throw<BusinessRuleException>()
            .Which.RuleCode.Should().Be("RN-03");
    }

    [Fact]
    public void Create_ShouldThrow_WhenEventsOverlapAtSameVenue()
    {
        var start = UtcNow.AddDays(5);
        var end = start.AddHours(4);

        var existing = Event.Create(
            "Existing Event",
            "An already scheduled event at the venue.",
            DefaultVenue,
            100,
            start,
            end,
            40m,
            EventType.Workshop,
            UtcNow,
            []);

        var act = () => Event.Create(
            "Overlapping Event",
            "This event overlaps with another one.",
            DefaultVenue,
            80,
            start.AddHours(1),
            end.AddHours(1),
            35m,
            EventType.Conference,
            UtcNow,
            [existing]);

        act.Should().Throw<BusinessRuleException>()
            .Which.RuleCode.Should().Be("RN-02");
    }

    [Fact]
    public void MarkAsCompleted_ShouldUpdateStatus_WhenEndDateHasPassed()
    {
        var start = UtcNow.AddDays(-2);
        var end = UtcNow.AddDays(-1);

        var @event = Event.Create(
            "Past Event",
            "An event that already finished.",
            DefaultVenue,
            50,
            start,
            end,
            25m,
            EventType.Workshop,
            start.AddDays(-5),
            []);

        @event.MarkAsCompleted(UtcNow);

        @event.Status.Should().Be(EventStatus.Completed);
    }
}
