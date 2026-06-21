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
            throw new BusinessRuleException("RESERVATION_CONFIRMED", "Esta reserva ya está confirmada.");
        }

        if (Status == ReservationStatus.Cancelled)
        {
            throw new BusinessRuleException("RESERVATION_CANCELLED", "No se puede confirmar una reserva cancelada.");
        }

        if (Status == ReservationStatus.Lost)
        {
            throw new BusinessRuleException("RESERVATION_LOST", "No se puede confirmar una reserva perdida.");
        }

        Status = ReservationStatus.Confirmed;
        ReservationCode = reservationCode;
    }

    public void Cancel(DateTime utcNow, bool applyPenalty)
    {
        if (Status == ReservationStatus.Cancelled)
        {
            throw new BusinessRuleException("RESERVATION_ALREADY_CANCELLED", "Esta reserva ya está cancelada.");
        }

        if (Status == ReservationStatus.Lost)
        {
            throw new BusinessRuleException("RESERVATION_LOST", "Esta reserva ya fue registrada como perdida.");
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
            throw new BusinessRuleException("EVENT_NOT_ACTIVE", "Solo se pueden reservar eventos activos.");
        }

        if (quantity < 1)
        {
            throw new BusinessRuleException("RESERVATION_QUANTITY", "La cantidad debe ser al menos 1.");
        }

        if (string.IsNullOrWhiteSpace(buyerName))
        {
            throw new BusinessRuleException("BUYER_NAME", "El nombre del comprador es obligatorio.");
        }

        if (!IsValidEmail(buyerEmail))
        {
            throw new BusinessRuleException("BUYER_EMAIL", "El correo del comprador no tiene un formato válido.");
        }

        if (@event.StartDateTime <= utcNow.AddHours(1))
        {
            throw new BusinessRuleException("RN-04", "No se permiten reservas cuando falta menos de 1 hora para iniciar el evento.");
        }

        var timeUntilStart = @event.StartDateTime - utcNow;
        var maxPerTransaction = GetMaxTicketsPerTransaction(@event, timeUntilStart);

        if (quantity > maxPerTransaction)
        {
            throw new BusinessRuleException(
                "RN-05",
                $"Solo puedes reservar hasta {maxPerTransaction} entradas por compra en este evento.");
        }

        if (quantity > @event.GetAvailableSeats())
        {
            throw new BusinessRuleException("CAPACITY", "No hay suficientes entradas disponibles.");
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
