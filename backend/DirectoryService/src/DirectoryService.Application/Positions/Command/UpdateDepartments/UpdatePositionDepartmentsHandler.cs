using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace DirectoryService.Application.Positions.Command.UpdateDepartments;

public class UpdatePositionDepartmentsHandler
{
    private readonly IPositionsRepository _positionsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<UpdatePositionDepartmentsCommand> _validator;
    private readonly ILogger<UpdatePositionDepartmentsHandler> _logger;
    private readonly HybridCache _cache;

    public UpdatePositionDepartmentsHandler(
        IPositionsRepository positionsRepository,
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        IValidator<UpdatePositionDepartmentsCommand> validator,
        ILogger<UpdatePositionDepartmentsHandler> logger,
        HybridCache cache)
    {
        _positionsRepository = positionsRepository;
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
        _cache = cache;
    }
    
    public async Task<Result<List<DepartmentPosition>, Errors>> Handle(
        UpdatePositionDepartmentsCommand command, CancellationToken cancellationToken)
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
        
        // существует ли позиция и активна ли она
        var positionResult = await _positionsRepository.GetByIdAsync(
            new PositionId(command.Id), cancellationToken);
        if (positionResult.IsFailure)
        {
            _logger.LogInformation("Position was not found.");
            
            return positionResult.Error.ToErrors();
        }
        
        // существуют ли депаратаменты и активны ли они
        var positionDepartmentIds = 
            command.UpdatePositionDepartmentsRequest.DepartmentIds.Select(id => new DepartmentId(id)).ToList();
        
        bool isAllLocationsExists = await _departmentsRepository.IsAllExistsAsync(positionDepartmentIds, cancellationToken);
        if (!isAllLocationsExists)
        {
            _logger.LogInformation("One or more departments were not found.");
            
            return Error.NotFound("departments.not.found", "Один или несколько департаментов не найдены").ToErrors();
        }
        
        var position = positionResult.Value;
        
        var departmentPositions = 
            positionDepartmentIds.Select(departmentId => new DepartmentPosition(departmentId, position.Id)).ToList();
        
        // обновление депаратментов позиции
        var updateDepartmentsResult = position.UpdateDepartments(departmentPositions);
        if (updateDepartmentsResult.IsFailure)
        {
            _logger.LogInformation("Failed to update DepartmentPositions.");
            
            return updateDepartmentsResult.Error.ToErrors();
        }
        
        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            transactionScope.Rollback();
            _logger.LogInformation("Failed to save changes. Transaction was rolled back.");
            
            return saveChangesResult.Error.ToErrors();
        }
        
        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            _logger.LogInformation("Failed to commit transaction.");
            return commitResult.Error.ToErrors();
        }
        
        // инвалидация кэша
        await _cache.RemoveByTagAsync(Constants.POSITION_CACHE_PREFIX, cancellationToken);
        
        return departmentPositions;
    }
}