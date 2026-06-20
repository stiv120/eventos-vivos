using EventosVivos.Application.DTOs;
using EventosVivos.Application.Mappings;
using EventosVivos.Application.Ports;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Exceptions;
using FluentValidation;

namespace EventosVivos.Application.Services;

public sealed class EventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IValidator<CreateEventRequest> _validator;

    public EventService(
        IEventRepository eventRepository,
        IVenueRepository venueRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        IValidator<CreateEventRequest> validator)
    {
        _eventRepository = eventRepository;
        _venueRepository = venueRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _validator = validator;
    }

    public async Task<EventResponse> CreateAsync(CreateEventRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var venue = await _venueRepository.GetByIdAsync(request.VenueId, cancellationToken)
            ?? throw new NotFoundException("Venue", request.VenueId);

        var overlapping = await _eventRepository.GetActiveOverlappingAsync(
            request.VenueId,
            request.StartDateTime,
            request.EndDateTime,
            cancellationToken: cancellationToken);

        var @event = Event.Create(
            request.Title,
            request.Description,
            venue,
            request.MaxCapacity,
            request.StartDateTime,
            request.EndDateTime,
            request.TicketPrice,
            request.Type,
            _dateTimeProvider.UtcNow,
            overlapping);

        await _eventRepository.AddAsync(@event, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EventMapper.ToResponse(@event);
    }

    public async Task<IReadOnlyList<EventResponse>> GetAllAsync(
        EventFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var events = await _eventRepository.GetAllAsync(new EventFilter
        {
            Type = filter.Type,
            StartDateFrom = filter.StartDateFrom,
            StartDateTo = filter.StartDateTo,
            VenueId = filter.VenueId,
            Status = filter.Status,
            TitleSearch = filter.TitleSearch
        }, cancellationToken);

        var utcNow = _dateTimeProvider.UtcNow;
        foreach (var @event in events)
        {
            @event.MarkAsCompleted(utcNow);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return events.Select(EventMapper.ToResponse).ToList();
    }

    public async Task<EventResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var @event = await _eventRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Event", id);

        @event.MarkAsCompleted(_dateTimeProvider.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EventMapper.ToResponse(@event);
    }

    public async Task<OccupancyReportResponse> GetOccupancyReportAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var @event = await _eventRepository.GetByIdAsync(eventId, cancellationToken)
            ?? throw new NotFoundException("Event", eventId);

        @event.MarkAsCompleted(_dateTimeProvider.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EventMapper.ToOccupancyReport(@event);
    }
}
