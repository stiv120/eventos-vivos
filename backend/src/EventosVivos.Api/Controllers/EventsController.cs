using EventosVivos.Application.DTOs;
using EventosVivos.Application.Services;
using EventosVivos.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EventsController : ControllerBase
{
    private readonly EventService _eventService;

    public EventsController(EventService eventService)
    {
        _eventService = eventService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request, CancellationToken cancellationToken)
    {
        var result = await _eventService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EventResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] EventType? type,
        [FromQuery] DateTime? startDateFrom,
        [FromQuery] DateTime? startDateTo,
        [FromQuery] int? venueId,
        [FromQuery] EventStatus? status,
        [FromQuery] string? titleSearch,
        CancellationToken cancellationToken)
    {
        var filter = new EventFilterRequest(type, startDateFrom, startDateTo, venueId, status, titleSearch);
        var result = await _eventService.GetAllAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _eventService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/occupancy-report")]
    [ProducesResponseType(typeof(OccupancyReportResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOccupancyReport(Guid id, CancellationToken cancellationToken)
    {
        var result = await _eventService.GetOccupancyReportAsync(id, cancellationToken);
        return Ok(result);
    }
}
