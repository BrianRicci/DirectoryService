using Core.Abstractions;
using DirectoryService.Application.Departments.Command.Create;
using DirectoryService.Application.Departments.Command.Delete;
using DirectoryService.Application.Departments.Command.Move;
using DirectoryService.Application.Departments.Command.Update;
using DirectoryService.Application.Departments.Queries;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.DepartmentLocations;
using Framework.EndpointResults;
using Microsoft.AspNetCore.Mvc;
using Shared.SharedKernel;

namespace DirectoryService.Presentation.Departments;

[ApiController]
[Route("api/departments")]
public class DepartmentController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<Envelope<Guid>>(201)]
    [ProducesResponseType<Envelope>(400)]
    [ProducesResponseType<Envelope>(500)]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreateDepartmentCommand> handler,
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDepartmentCommand(request);

        return await handler.Handle(command, cancellationToken);
    }
    
    [HttpPatch("{departmentId}/locations")]
    public async Task<EndpointResult<List<DepartmentLocation>>> UpdateLocations(
        [FromRoute] Guid departmentId,
        [FromServices] ICommandHandler<List<DepartmentLocation>, UpdateDepartmentLocationsCommand> handler,
        [FromBody] UpdateDepartmentLocationsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDepartmentLocationsCommand(departmentId, request);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpPatch("{departmentId}/parent")]
    public async Task<EndpointResult> MoveDepartment(
        [FromRoute] Guid departmentId,
        [FromServices] ICommandHandler<MoveDepartmentCommand> handler,
        [FromBody] MoveDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new MoveDepartmentCommand(departmentId, request);
        
        return await handler.Handle(command, cancellationToken);
    }
    
    [HttpGet("top-positions")]
    public async Task<EndpointResult<GetDepartmentsTopDto?>> GetTopPositions(
        [FromServices] GetDepartmentsTopHandlerDapper handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(cancellationToken);
    }
    
    [HttpGet("roots")]
    public async Task<EndpointResult<GetDepartmentRootsDto?>> GetRoots(
        [FromQuery] GetDepartmentRootsRequest request,
        [FromServices] GetDepartmentRootsHandlerDapper handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(request, cancellationToken);
    }
    
    [HttpGet("{parentId:guid}/children")]
    public async Task<EndpointResult<GetDepartmentChildsDto>> GetChilds(
        [FromRoute] Guid parentId,
        [FromQuery] PaginationRequest paginationRequest,
        [FromServices] GetDepartmentChildsHandlerDapper handler,
        CancellationToken cancellationToken)
    {
        var request = new GetDepartmentChildsRequest(parentId, paginationRequest);
        return await handler.Handle(request, cancellationToken);
    }
    
    [HttpGet("names")]
    public async Task<EndpointResult<PaginationResponse<DepartmentNamesDto>>> GetNames(
        [FromQuery] GetDepartmentNamesRequest request,
        [FromServices] GetDepartmentNamesForFilterDapper handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(request, cancellationToken);
    }
    
    [HttpDelete("{departmentId:guid}")]
    public async Task<EndpointResult<Guid>> Delete(
        [FromRoute] Guid departmentId,
        [FromServices] ICommandHandler<Guid, DeleteDepartmentCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new DeleteDepartmentCommand(departmentId);
        
        return await handler.Handle(command, cancellationToken);
    }
}