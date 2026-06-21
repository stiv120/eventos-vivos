using EventosVivos.Application.DTOs;
using EventosVivos.Domain.Entities;

namespace EventosVivos.Application.Mappings;

public static class ReservationMapper
{
    public static ReservationResponse ToResponse(Reservation reservation) =>
        new(
            reservation.Id,
            reservation.EventId,
            reservation.Quantity,
            reservation.BuyerName,
            reservation.BuyerEmail,
            reservation.Status,
            reservation.ReservationCode,
            reservation.CreatedAt,
            reservation.CancelledAt);
}
