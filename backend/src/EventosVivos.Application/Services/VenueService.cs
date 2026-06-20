using EventosVivos.Application.DTOs;
using EventosVivos.Application.Ports;

namespace EventosVivos.Application.Services;

public sealed class VenueService
{
    private readonly IVenueRepository _venueRepository;

    public VenueService(IVenueRepository venueRepository)
    {
        _venueRepository = venueRepository;
    }

    public async Task<IReadOnlyList<VenueResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var venues = await _venueRepository.GetAllAsync(cancellationToken);
        return venues.Select(v => new VenueResponse(v.Id, v.Name, v.Capacity, v.City)).ToList();
    }
}
