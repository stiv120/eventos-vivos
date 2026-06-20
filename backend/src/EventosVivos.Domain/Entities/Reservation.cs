using EventosVivos.Domain.Enums;
using EventosVivos.Domain.Exceptions;

namespace EventosVivos.Domain.Entities;

public sealed class Reservation
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Event? Event { get; private set; }
    public int Quantity { get; private set; }
    public string BuyerName { get; private set; } = string.Empty;
    public string BuyerEmail { get; private set; } = string.Empty;
    public ReservationStatus Status { get; private set; }
    public string? ReservationCode { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private Reservation()
    {
    }

    public static Reservation Create(
        Event @event,
        int quantity,
        string buyerName,
        string buyerEmail,
        DateTime utcNow)
    {
        ValidateReservationRules(@event, quantity, buyerName, buyerEmail, utcNow);

        return new Reservation
        {
            Id = Guid.NewGuid(),
            EventId = @event.Id,
            Event = @event,
            Quantity = quantity,
            BuyerName = buyerName.Trim(),
            BuyerEmail = buyerEmail.Trim().ToLowerInvariant(),
            Status = ReservationStatus.PendingPayment,
            CreatedAt = utcNow
        };
    }

    public void ConfirmPayment(string reservationCode)
    {
        if (Status == ReservationStatus.Confirmed)
        {
            throw new BusinessRuleException("RESERVATION_CONFIRMED", "Reservation is already confirmed.");
        }

        if (Status == ReservationStatus.Cancelled)
        {
            throw new BusinessRuleException("RESERVATION_CANCELLED", "Cancelled reservations cannot be confirmed.");
        }

        if (Status == ReservationStatus.Lost)
        {
            throw new BusinessRuleException("RESERVATION_LOST", "Lost reservations cannot be confirmed.");
        }

        Status = ReservationStatus.Confirmed;
        ReservationCode = reservationCode;
    }

    public void Cancel(DateTime utcNow, bool applyPenalty)
    {
        if (Status == ReservationStatus.Cancelled)
        {
            throw new BusinessRuleException("RESERVATION_ALREADY_CANCELLED", "Reservation is already cancelled.");
        }

        if (Status == ReservationStatus.Lost)
        {
            throw new BusinessRuleException("RESERVATION_LOST", "Lost reservations cannot be cancelled again.");
        }

        if (applyPenalty && Status == ReservationStatus.Confirmed)
        {
            Status = ReservationStatus.Lost;
        }
        else
        {
            Status = ReservationStatus.Cancelled;
        }

        CancelledAt = utcNow;
    }

    private static void ValidateReservationRules(
        Event @event,
        int quantity,
        string buyerName,
        string buyerEmail,
        DateTime utcNow)
    {
        if (!@event.IsActive())
        {
            throw new BusinessRuleException("EVENT_NOT_ACTIVE", "Reservations are only allowed for active events.");
        }

        if (quantity < 1)
        {
            throw new BusinessRuleException("RESERVATION_QUANTITY", "Quantity must be at least 1.");
        }

        if (string.IsNullOrWhiteSpace(buyerName))
        {
            throw new BusinessRuleException("BUYER_NAME", "Buyer name is required.");
        }

        if (!IsValidEmail(buyerEmail))
        {
            throw new BusinessRuleException("BUYER_EMAIL", "Buyer email format is invalid.");
        }

        if (@event.StartDateTime <= utcNow.AddHours(1))
        {
            throw new BusinessRuleException("RN-04", "Reservations are not allowed within 1 hour of event start.");
        }

        var timeUntilStart = @event.StartDateTime - utcNow;
        var maxPerTransaction = GetMaxTicketsPerTransaction(@event, timeUntilStart);

        if (quantity > maxPerTransaction)
        {
            throw new BusinessRuleException(
                "RN-05",
                $"Maximum {maxPerTransaction} tickets allowed per transaction for this event.");
        }

        if (quantity > @event.GetAvailableSeats())
        {
            throw new BusinessRuleException("CAPACITY", "Not enough tickets available for this event.");
        }
    }

    private static int GetMaxTicketsPerTransaction(Event @event, TimeSpan timeUntilStart)
    {
        var limits = new List<int> { int.MaxValue };

        if (timeUntilStart < TimeSpan.FromHours(24))
        {
            limits.Add(5);
        }

        if (@event.TicketPrice > 100)
        {
            limits.Add(10);
        }

        return limits.Min();
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
