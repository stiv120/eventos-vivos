using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Exceptions;

namespace EventosVivos.Domain.Entities;

public sealed class Event
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int VenueId { get; private set; }
    public Venue? Venue { get; private set; }
    public int MaxCapacity { get; private set; }
    public DateTime StartDateTime { get; private set; }
    public DateTime EndDateTime { get; private set; }
    public decimal TicketPrice { get; private set; }
    public EventType Type { get; private set; }
    public EventStatus Status { get; private set; }
    public ICollection<Reservation> Reservations { get; private set; } = new List<Reservation>();

    private Event()
    {
    }

    public static Event Create(
        string title,
        string description,
        Venue venue,
        int maxCapacity,
        DateTime startDateTime,
        DateTime endDateTime,
        decimal ticketPrice,
        EventType type,
        DateTime utcNow,
        IEnumerable<Event> overlappingActiveEvents)
    {
        ValidateCreationRules(
            title,
            description,
            venue,
            maxCapacity,
            startDateTime,
            endDateTime,
            ticketPrice,
            utcNow,
            overlappingActiveEvents);

        return new Event
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description.Trim(),
            VenueId = venue.Id,
            Venue = venue,
            MaxCapacity = maxCapacity,
            StartDateTime = startDateTime,
            EndDateTime = endDateTime,
            TicketPrice = ticketPrice,
            Type = type,
            Status = EventStatus.Active
        };
    }

    public void MarkAsCompleted(DateTime utcNow)
    {
        if (Status == EventStatus.Active && utcNow > EndDateTime)
        {
            Status = EventStatus.Completed;
        }
    }

    public void Cancel()
    {
        if (Status != EventStatus.Active)
        {
            throw new BusinessRuleException("EVENT_CANCEL", "Only active events can be cancelled.");
        }

        Status = EventStatus.Cancelled;
    }

    public int GetOccupiedSeats()
    {
        return Reservations
            .Where(r => r.Status is ReservationStatus.PendingPayment or ReservationStatus.Confirmed or ReservationStatus.Lost)
            .Sum(r => r.Quantity);
    }

    public int GetConfirmedSeats()
    {
        return Reservations
            .Where(r => r.Status == ReservationStatus.Confirmed)
            .Sum(r => r.Quantity);
    }

    public int GetAvailableSeats()
    {
        var heldSeats = Reservations
            .Where(r => r.Status is ReservationStatus.PendingPayment or ReservationStatus.Confirmed)
            .Sum(r => r.Quantity);

        return MaxCapacity - heldSeats;
    }

    public bool IsActive() => Status == EventStatus.Active;

    private static void ValidateCreationRules(
        string title,
        string description,
        Venue venue,
        int maxCapacity,
        DateTime startDateTime,
        DateTime endDateTime,
        decimal ticketPrice,
        DateTime utcNow,
        IEnumerable<Event> overlappingActiveEvents)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length < 5 || title.Trim().Length > 100)
        {
            throw new BusinessRuleException("EVENT_TITLE", "Title must be between 5 and 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length < 10 || description.Trim().Length > 500)
        {
            throw new BusinessRuleException("EVENT_DESCRIPTION", "Description must be between 10 and 500 characters.");
        }

        if (maxCapacity <= 0)
        {
            throw new BusinessRuleException("EVENT_CAPACITY", "Max capacity must be a positive integer.");
        }

        if (maxCapacity > venue.Capacity)
        {
            throw new BusinessRuleException("RN-01", "Event capacity cannot exceed venue capacity.");
        }

        if (startDateTime <= utcNow)
        {
            throw new BusinessRuleException("EVENT_START", "Start date must be in the future.");
        }

        if (endDateTime <= startDateTime)
        {
            throw new BusinessRuleException("EVENT_END", "End date must be after start date.");
        }

        if (ticketPrice <= 0)
        {
            throw new BusinessRuleException("EVENT_PRICE", "Ticket price must be a positive decimal.");
        }

        if (IsWeekend(startDateTime) && startDateTime.TimeOfDay > new TimeSpan(22, 0, 0))
        {
            throw new BusinessRuleException("RN-03", "Weekend events cannot start after 22:00.");
        }

        if (overlappingActiveEvents.Any(e => HasScheduleOverlap(startDateTime, endDateTime, e.StartDateTime, e.EndDateTime)))
        {
            throw new BusinessRuleException("RN-02", "Active events cannot overlap at the same venue.");
        }
    }

    private static bool IsWeekend(DateTime dateTime) =>
        dateTime.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    private static bool HasScheduleOverlap(
        DateTime startA,
        DateTime endA,
        DateTime startB,
        DateTime endB) =>
        startA < endB && endA > startB;
}
