using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Extentions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments.Command.Delete;

public class DeleteDepartmentHandler : ICommandHandler<Guid, DeleteDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IPositionsRepository _positionsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<DeleteDepartmentCommand> _validator;
    private readonly ILogger<DeleteDepartmentHandler> _logger;

    public DeleteDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IPositionsRepository positionsRepository,
        ITransactionManager transactionManager,
        IValidator<DeleteDepartmentCommand> validator,
        ILogger<DeleteDepartmentHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _positionsRepository = positionsRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Errors>> Handle(DeleteDepartmentCommand command, CancellationToken cancellationToken)
    {
        // валидация
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToList();
        }
        
        // открытие транзакции
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            _logger.LogInformation("Failed to begin transaction.");
            return transactionScopeResult.Error.ToErrors();
        }
        
        using var transactionScope = transactionScopeResult.Value;
        
        // проверка есть ли department
        var departmentResult = await _departmentsRepository.GetByIdWithLock(new DepartmentId(command.DepartmentId), cancellationToken);
        if (departmentResult.IsFailure)
        {
            _logger.LogInformation("Department was not found.");
            transactionScope.Rollback();
            return departmentResult.Error;
        }
        
        var department = departmentResult.Value;

        var oldPath = department.Path;
        
        var lockDescendants = await _departmentsRepository.LockDescendants(oldPath, cancellationToken);
        if (lockDescendants.IsFailure)
        {
            _logger.LogInformation("Failed to lock descendants.");
            transactionScope.Rollback(); 
            return lockDescendants.Error;
        }

        Location? location = null;
        Position? position = null;
        
        var locationDtoResult = await _locationsRepository.GetLocationsRelatedToDepartmentAsync(
            department.Id, cancellationToken);
        if (locationDtoResult.IsFailure)
        {
            _logger.LogInformation("Failed to get locations related to department.");
            transactionScope.Rollback();
            return locationDtoResult.Error.ToErrors();
        }
        
        var positionDtoResult = await _positionsRepository.GetPositionsRelatedToDepartmentAsync(
            department.Id, cancellationToken);
        if (positionDtoResult.IsFailure)
        {
            _logger.LogInformation("Failed to get positions related to department.");
            transactionScope.Rollback();
            return positionDtoResult.Error.ToErrors();
        }
        
        if (locationDtoResult.Value.Locations.Count == 1)
        {
            var locationDtos = locationDtoResult.Value.Locations;
            
            location = locationDtos
                .Select(dto =>
                {
                    var id = new LocationId(dto.LocationId);
                    var name = LocationName.Create(dto.Name).Value;
                    var address = LocationAddress.Create(
                        dto.Address.Country,
                        dto.Address.Region,
                        dto.Address.City,
                        dto.Address.Street,
                        dto.Address.House).Value;
                    var timezone = LocationTimezone.Create(dto.Timezone).Value;
                    
                    var locationResult = Location.Create(id, name, address, timezone);

                    return locationResult.Value;
                })
                .First();
        }

        if (positionDtoResult.Value.Positions.Count == 1)
        {
            var positionDtos = positionDtoResult.Value.Positions;
            
            position = positionDtos
                .Select(dto =>
                {
                    var id = new PositionId(dto.PositionId);
                    var name = PositionName.Create(dto.Name).Value;
                    var description = PositionDescription.Create(dto.Description).Value;

                    var departmentPositions = dto.Departments
                        .Select(d => 
                            new DepartmentPosition(new DepartmentId(d.DepartmentId), id))
                        .ToList();
                    
                    var positionResult = Position.Create(id, name, description, departmentPositions);

                    return positionResult.Value;
                })
                .First();
        }
        
        location?.Delete();
        position?.Delete();
        
        department.Delete();
        
        var saveChanges = await _departmentsRepository.SaveChangesAsync(cancellationToken);
        if (saveChanges.IsFailure)
        {
            transactionScope.Rollback();
            return saveChanges.Error;
        }
        
        int depthDelta = 0;
        
        var updateDescendantDepartmentsResult = await _departmentsRepository.BulkUpdateDescendantsPath(
            oldPath,
            department.Path,
            depthDelta,
            cancellationToken);
        if (updateDescendantDepartmentsResult.IsFailure)
        {
            _logger.LogInformation("Failed to update descendant departments.");
            transactionScope.Rollback();
            return updateDescendantDepartmentsResult.Error;
        }
        
        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            _logger.LogInformation("Failed to commit transaction.");
            transactionScope.Rollback();
            return commitResult.Error.ToErrors();
        }
        
        _logger.LogInformation("Department {DepartmentId} softly deleted", command.DepartmentId);
        
        return department.Id.Value;
    }
}