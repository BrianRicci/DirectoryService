using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace DirectoryService.Application.Departments.Command.Create;

public class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateDepartmentCommand> _validator;
    private readonly ILogger<CreateDepartmentHandler> _logger;
    private readonly HybridCache _cache;

    public CreateDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IValidator<CreateDepartmentCommand> validator,
        ILogger<CreateDepartmentHandler> logger,
        HybridCache cache)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
        _cache = cache;
    }
    
    public async Task<Result<Guid, Errors>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        // валидация
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToList();
        }
        
        // создание сущности
        var departmentName = DepartmentName.Create(command.CreateDepartmentRequest.Name).Value;
        
        var departmentIdentifier = DepartmentIdentifier.Create(command.CreateDepartmentRequest.Identifier).Value;
        
        DepartmentPath departmentPath;
        
        short departmentDepth = 0;
        
        Guid? departmentParentId = command.CreateDepartmentRequest.ParentId; 
        
        if (departmentParentId.HasValue)
        {
            var parentDepartmentResult = await _departmentsRepository
                .GetByIdAsync(new DepartmentId(departmentParentId.Value), cancellationToken);
            if (parentDepartmentResult.IsFailure)
                return parentDepartmentResult.Error.ToErrors();
            
            departmentDepth = (short)(parentDepartmentResult.Value.Depth + 1); // приведение int единицы к short
            
            var parentDepartment = parentDepartmentResult.Value; 
            
            departmentPath = parentDepartment.Path.CreateChild(departmentIdentifier).Value;
        } else
        {
            departmentPath = DepartmentPath.CreateParent(departmentIdentifier).Value;
        }
        
        var departmentLocationIds = 
            command.CreateDepartmentRequest.LocationIds.Select(id => new LocationId(id)).ToList();
        
        bool isAllLocationsExists = await _locationsRepository
            .IsAllExistsAsync(departmentLocationIds, cancellationToken);
        if (!isAllLocationsExists)
        {
            _logger.LogInformation("One or more locations were not found.");
            
            return Error.NotFound("locations.not.found", "Одна или несколько локаций не найдены").ToErrors();
        }
        
        var departmentId = new DepartmentId(Guid.NewGuid());
        
        var departmentLocations = 
            departmentLocationIds.Select(locationId => new DepartmentLocation(departmentId, locationId)).ToList();
        
        var department = departmentParentId is null
            ? Department.CreateParent(
            departmentName,
            departmentIdentifier,
            departmentPath,
            departmentDepth,
            departmentLocations,
            departmentId).Value
            : Department.CreateChild(
            new DepartmentId(departmentParentId.Value),
            departmentName,
            departmentIdentifier,
            departmentPath,
            departmentDepth,
            departmentLocations,
            departmentId).Value;
        
        // сохранение сущности в БД
        var savedDepartmentResult = await _departmentsRepository.AddAsync(department, cancellationToken);
        if (savedDepartmentResult.IsFailure)
        {
            return savedDepartmentResult.Error.ToErrors();
        }

        // инвалидация кэша
        await _cache.RemoveByTagAsync(Constants.DEPARTMENT_CACHE_PREFIX, cancellationToken);
        
        // логирование
        _logger.LogInformation("Department created with id: {departmentId}", department.Id.Value);
        
        return savedDepartmentResult.Value;
    }
}