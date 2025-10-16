using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.Command.CreateLocation;
using DirectoryService.Application.Locations.Queries;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Locations;
using DirectoryService.Presentation.EndpointResults;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace DirectoryService.Presentation.Locations;

[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<Envelope<Guid>>(201)]
    [ProducesResponseType<Envelope>(400)]
    [ProducesResponseType<Envelope>(500)]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handler,
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(request);

        return await handler.Handle(command, cancellationToken);
    }
    
    [HttpGet("/{locationId:guid}")]
    public async Task<ActionResult<GetLocationDto>> GetById(
        [FromRoute] Guid locationId,
        [FromServices] GetByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var location = await handler.Handle(new GetLocationByIdRequest(locationId), cancellationToken);
        return Ok(location);
    }
    
    [HttpGet("/{locationId:guid}/dapper")]
    public async Task<ActionResult<GetLocationDto>> GetByIdDapper(
        [FromRoute] Guid locationId,
        [FromServices] GetByIdHandlerDapper handler,
        CancellationToken cancellationToken)
    {
        var location = await handler.Handle(new GetLocationByIdRequest(locationId), cancellationToken);
        return Ok(location);
    }
}