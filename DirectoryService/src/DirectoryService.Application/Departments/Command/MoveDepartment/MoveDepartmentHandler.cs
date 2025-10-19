﻿using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Extentions;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments.Command.MoveDepartment;

public class MoveDepartmentHandler : ICommandHandler<MoveDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<MoveDepartmentCommand> _validator;
    private readonly ILogger<MoveDepartmentHandler> _logger;

    public MoveDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        IValidator<MoveDepartmentCommand> validator,
        ILogger<MoveDepartmentHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }
    
    public async Task<UnitResult<Errors>> Handle(MoveDepartmentCommand command, CancellationToken cancellationToken)
    {
        // валидация
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToList();
        }
        
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            _logger.LogInformation("Failed to begin transaction.");
            return transactionScopeResult.Error.ToErrors();
        }
        
        using var transactionScope = transactionScopeResult.Value;
        
        var departmentResult = await _departmentsRepository.GetByIdWithLock(new DepartmentId(command.DepartmentId), cancellationToken);
        if (departmentResult.IsFailure)
        {
            _logger.LogInformation("Department was not found.");
            return departmentResult.Error;
        }
        
        var department = departmentResult.Value;

        var oldPath = department.Path;
        
        var lockDescendants = await _departmentsRepository.LockDescendants(oldPath, cancellationToken);
        if (lockDescendants.IsFailure)
        {
            _logger.LogInformation("Failed to lock descendants.");
            return lockDescendants.Error;
        }

        var parentId = command.MoveDepartmentRequest.ParentId;

        var descendantsResult = await _departmentsRepository.GetDescendantsByPath(oldPath, cancellationToken);
        if (descendantsResult.IsFailure)
        {
            _logger.LogInformation("Failed to get descendants.");
            return descendantsResult.Error;
        }

        List<Department> descendants = descendantsResult.Value;

        Department? parentDepartment = null;
        
        if (parentId.HasValue)
        {
            bool isParentContainedInDescendants = descendants.Any(d => d.Id.Value == parentId);
            if (isParentContainedInDescendants)
            {
                return Error.Validation(
                    "parent.contained.in.descendants", 
                    "The parent department cannot be contained in the descendants").ToErrors();
            }
            
            var parentDepartmentResult = await _departmentsRepository.GetByIdWithLock(new DepartmentId(parentId.Value), cancellationToken);
            if (parentDepartmentResult.IsFailure)
            {
                _logger.LogInformation("Parent department was not found.");
                return parentDepartmentResult.Error;
            }
            
            parentDepartment = parentDepartmentResult.Value;
        }

        int depthDelta = department.SetParent(parentDepartment);

        var saveChanges = await _departmentsRepository.SaveChangesAsync(cancellationToken);
        if (saveChanges.IsFailure)
        {
            transactionScope.Rollback();
            return saveChanges.Error;
        }
        
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
        
        return UnitResult.Success<Errors>();
    }
}