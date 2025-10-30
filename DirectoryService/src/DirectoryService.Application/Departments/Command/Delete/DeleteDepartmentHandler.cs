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

        var softDeleteLocationsResult = await _locationsRepository.SoftDeleteLocationsRelatedToDepartmentAsync(
            department.Id, cancellationToken);
        if (softDeleteLocationsResult.IsFailure)
        {
            _logger.LogInformation("Failed to delete locations.");
            transactionScope.Rollback();
            return softDeleteLocationsResult.Error.ToErrors();
        }
        
        var softDeletePositionsResult = await _positionsRepository.SoftDeletePositionsRelatedToDepartmentAsync(
            department.Id, cancellationToken);
        if (softDeletePositionsResult.IsFailure)
        {
            _logger.LogInformation("Failed to delete positions.");
            transactionScope.Rollback();
            return softDeletePositionsResult.Error.ToErrors();
        }
        
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