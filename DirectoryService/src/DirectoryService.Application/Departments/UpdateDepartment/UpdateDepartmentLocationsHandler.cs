using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Extentions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments.UpdateDepartment;

public class UpdateDepartmentLocationsHandler : ICommandHandler<UpdateDepartmentLocationsCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<UpdateDepartmentLocationsCommand> _validator;
    private readonly ILogger<UpdateDepartmentLocationsHandler> _logger;

    public UpdateDepartmentLocationsHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        ITransactionManager transactionManager,
        IValidator<UpdateDepartmentLocationsCommand> validator,
        ILogger<UpdateDepartmentLocationsHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }
    
    public async Task<UnitResult<Errors>> Handle(UpdateDepartmentLocationsCommand command, CancellationToken cancellationToken)
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
        
        // существует ли подразделение и активно ли оно
        var departmentResult = await _departmentsRepository.GetByIdAsync(
            new DepartmentId(command.Id), cancellationToken);
        if (departmentResult.IsFailure)
        {
            _logger.LogInformation("Department was not found.");
            
            return departmentResult.Error;
        }
        
        // существуют ли локации и активны ли они
        var departmentLocationIds = 
            command.UpdateDepartmentLocationsRequest.LocationIds.Select(id => new LocationId(id)).ToList();
        
        bool isAllLocationsExists = await _locationsRepository.IsAllExistsAsync(departmentLocationIds, cancellationToken);
        if (!isAllLocationsExists)
        {
            _logger.LogInformation("One or more locations were not found.");
            
            return Error.NotFound("locations.not.found", "Одна или несколько локаций не найдены").ToErrors();
        }
        
        var department = departmentResult.Value;
        
        var departmentLocations = 
            departmentLocationIds.Select(locationId => new DepartmentLocation(department.Id, locationId)).ToList();
        
        // обновление локаций депаратмента
        var updateLocationsResult = department.UpdateLocations(departmentLocations);
        if (updateLocationsResult.IsFailure)
        {
            _logger.LogInformation("Failed to update DepartmentLocations.");
            
            return updateLocationsResult.Error.ToErrors();
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
        
        return UnitResult.Success<Errors>();
    }
}