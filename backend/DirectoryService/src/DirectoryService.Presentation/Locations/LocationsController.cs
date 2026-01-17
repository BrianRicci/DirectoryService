using Core.Abstractions;
using DirectoryService.Application.Locations.Command.Create;
using DirectoryService.Application.Locations.Command.Update;
using DirectoryService.Application.Locations.Queries;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using Framework.EndpointResults;
using Microsoft.AspNetCore.Mvc;
using Shared.SharedKernel;

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
    
    [HttpPatch("{locationId}")]
    public async Task<EndpointResult<Location>> Update(
        [FromRoute] Guid locationId,
        [FromServices] ICommandHandler<Location, UpdateLocationCommand> handler,
        [FromBody] UpdateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLocationCommand(locationId, request);
        
        return await handler.Handle(command, cancellationToken);
    }
    
    [HttpGet]
    public async Task<EndpointResult<GetLocationsDto?>> Get(
        [FromQuery] GetLocationsRequest request,
        [FromServices] GetLocationsHandlerDapper handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(request, cancellationToken);
    }
    
    [HttpGet("{locationId:guid}")]
    public async Task<EndpointResult<GetLocationDto>> GetById(
        [FromRoute] Guid locationId,
        [FromServices] GetLocationByIdHandler handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(new GetLocationByIdRequest(locationId), cancellationToken);
    }
    
    [HttpGet("{locationId:guid}/dapper")]
    public async Task<EndpointResult<GetLocationDto>> GetByIdDapper(
        [FromRoute] Guid locationId,
        [FromServices] GetByIdHandlerDapper handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(new GetLocationByIdRequest(locationId), cancellationToken);
    }
}