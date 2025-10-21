using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.Command.CreateDepartment;
using DirectoryService.Application.Departments.Command.MoveDepartment;
using DirectoryService.Application.Departments.Command.UpdateDepartment;
using DirectoryService.Application.Departments.Queries;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Presentation.EndpointResults;
using Microsoft.AspNetCore.Mvc;
using Shared;

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
    public async Task<ActionResult<GetDepartmentsTopDto?>> GetTopPositions(
        [FromServices] GetDepartmentsTopHandlerDapper handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(cancellationToken);
    }
    
    [HttpGet("roots")]
    public async Task<ActionResult<GetDepartmentRootsDto?>> GetRoots(
        [FromQuery] GetDepartmentRootsRequest request,
        [FromServices] GetDepartmentRootsHandlerDapper handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(request, cancellationToken);
    }
}