using DirectoryService.Application.Locations;
using DirectoryService.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Locations;

[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    private readonly ILocationsService _locationsService;

    public LocationsController(ILocationsService locationsService)
    {
        _locationsService = locationsService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateLocationDto locationDto,
        CancellationToken cancellationToken)
    {
        var locationId = await _locationsService.Create(locationDto, cancellationToken);

        return Ok(locationId);
    }
}