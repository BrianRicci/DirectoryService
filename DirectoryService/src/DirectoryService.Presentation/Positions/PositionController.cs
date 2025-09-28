using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Positions;
using DirectoryService.Application.Positions.CreatePosition;
using DirectoryService.Contracts.Positions;
using DirectoryService.Presentation.EndpointResults;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace DirectoryService.Presentation.Positions;

[ApiController]
[Route("api/positions")]
public class PositionController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<Envelope<Guid>>(201)]
    [ProducesResponseType<Envelope>(400)]
    [ProducesResponseType<Envelope>(500)]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreatePositionCommand> handler,
        [FromBody] CreatePositionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePositionCommand(request);

        return await handler.Handle(command, cancellationToken);
    }
}