using Core.Abstractions;
using DirectoryService.Application.Positions.Command.Create;
using DirectoryService.Application.Positions.Queries;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Positions;
using Framework.EndpointResults;
using Microsoft.AspNetCore.Mvc;
using Shared.SharedKernel;

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
    
    [HttpGet]
    public async Task<EndpointResult<PaginationResponse<GetPositionDto>?>> Get(
        [FromQuery] GetPositionsRequest request,
        [FromServices] GetPositionsHandlerDapper handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(request, cancellationToken);
    }
    
    [HttpGet("{positionId:guid}")]
    public async Task<EndpointResult<GetPositionDto>> GetByIdDapper(
        [FromRoute] Guid positionId,
        [FromServices] GetPositionByIdHandlerDapper handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(new GetPositionByIdRequest(positionId), cancellationToken);
    }
}