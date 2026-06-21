using EventosVivos.Domain.Enums;

namespace EventosVivos.Application.DTOs;

public sealed record CreateEventRequest(
    string Title,
    string Description,
    int VenueId,
    int MaxCapacity,
    DateTime StartDateTime,
    DateTime EndDateTime,
    decimal TicketPrice,
    EventType Type);

public sealed record EventResponse(
    Guid Id,
    string Title,
    string Description,
    int VenueId,
    string VenueName,
    string VenueCity,
    int MaxCapacity,
    DateTime StartDateTime,
    DateTime EndDateTime,
    decimal TicketPrice,
    EventType Type,
    EventStatus Status,
    int AvailableSeats);

public sealed record EventFilterRequest(
    EventType? Type,
    DateTime? StartDateFrom,
    DateTime? StartDateTo,
    int? VenueId,
    EventStatus? Status,
    string? TitleSearch);

public sealed record CreateReservationRequest(
    Guid EventId,
    int Quantity,
    string BuyerName,
    string BuyerEmail);

public sealed record ReservationResponse(
    Guid Id,
    Guid EventId,
    int Quantity,
    string BuyerName,
    string BuyerEmail,
    ReservationStatus Status,
    string? ReservationCode,
    DateTime CreatedAt,
    DateTime? CancelledAt);

public sealed record OccupancyReportResponse(
    Guid EventId,
    string EventTitle,
    int TotalSoldTickets,
    int AvailableTickets,
    decimal OccupancyPercentage,
    decimal TotalRevenue,
    EventStatus Status);

public sealed record VenueResponse(int Id, string Name, int Capacity, string City);
