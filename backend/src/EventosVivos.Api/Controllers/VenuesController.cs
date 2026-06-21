using EventosVivos.Application.DTOs;
using EventosVivos.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class VenuesController : ControllerBase
{
    private readonly VenueService _venueService;

    public VenuesController(VenueService venueService)
    {
        _venueService = venueService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VenueResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _venueService.GetAllAsync(cancellationToken);
        return Ok(result);
    }
}
