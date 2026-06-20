using EventosVivos.Application.DTOs;
using EventosVivos.Application.Mappings;
using EventosVivos.Application.Ports;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Exceptions;
using FluentValidation;

namespace EventosVivos.Application.Services;

public sealed class ReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IReservationCodeGenerator _codeGenerator;
    private readonly IValidator<CreateReservationRequest> _validator;

    public ReservationService(
        IReservationRepository reservationRepository,
        IEventRepository eventRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        IReservationCodeGenerator codeGenerator,
        IValidator<CreateReservationRequest> validator)
    {
        _reservationRepository = reservationRepository;
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _codeGenerator = codeGenerator;
        _validator = validator;
    }

    public async Task<ReservationResponse> CreateAsync(
        CreateReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var @event = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new NotFoundException("Event", request.EventId);

        @event.MarkAsCompleted(_dateTimeProvider.UtcNow);

        var reservation = Reservation.Create(
            @event,
            request.Quantity,
            request.BuyerName,
            request.BuyerEmail,
            _dateTimeProvider.UtcNow);

        await _reservationRepository.AddAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ReservationMapper.ToResponse(reservation);
    }

    public async Task<ReservationResponse> ConfirmPaymentAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken)
            ?? throw new NotFoundException("Reservation", reservationId);

        var code = await _codeGenerator.GenerateUniqueCodeAsync(cancellationToken);
        reservation.ConfirmPayment(code);

        await _reservationRepository.UpdateAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ReservationMapper.ToResponse(reservation);
    }

    public async Task<ReservationResponse> CancelAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken)
            ?? throw new NotFoundException("Reservation", reservationId);

        var @event = reservation.Event
            ?? await _eventRepository.GetByIdAsync(reservation.EventId, cancellationToken)
            ?? throw new NotFoundException("Event", reservation.EventId);

        var utcNow = _dateTimeProvider.UtcNow;
        var hoursUntilEvent = (@event.StartDateTime - utcNow).TotalHours;

        var applyPenalty = reservation.Status == Domain.Enums.ReservationStatus.Confirmed
            && hoursUntilEvent < 48;

        reservation.Cancel(utcNow, applyPenalty);

        await _reservationRepository.UpdateAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ReservationMapper.ToResponse(reservation);
    }

    public async Task<ReservationResponse> GetByIdAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken)
            ?? throw new NotFoundException("Reservation", reservationId);

        return ReservationMapper.ToResponse(reservation);
    }
}
