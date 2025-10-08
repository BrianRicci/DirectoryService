using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.MoveDepartment;
using DirectoryService.Application.Departments.UpdateDepartment;
using DirectoryService.Contracts.Departments;
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
    public async Task<EndpointResult> UpdateLocations(
        [FromRoute] Guid departmentId,
        [FromServices] ICommandHandler<UpdateDepartmentLocationsCommand> handler,
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
}