using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Extentions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private const char SEPARATOR = '.';
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateDepartmentCommand> _validator;
    private readonly ILogger<CreateDepartmentHandler> _logger;
    
    public CreateDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IValidator<CreateDepartmentCommand> validator,
        ILogger<CreateDepartmentHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
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
                return parentDepartmentResult.Error;
            
            departmentDepth = (short)(parentDepartmentResult.Value.Depth + 1); // приведение int единицы к short
            
            departmentPath = DepartmentPath
                .Create(parentDepartmentResult.Value.Path.Value + SEPARATOR + departmentIdentifier.Value).Value;
        } else
        {
            departmentPath = DepartmentPath
                .Create(departmentIdentifier.Value).Value;
        }
        
        var departmentLocationIds = 
            command.CreateDepartmentRequest.LocationIds.Select(id => new LocationId(id)).ToList();
        
        bool isAllLocationsExists = await _locationsRepository
            .IsAllExistsAsync(departmentLocationIds, cancellationToken);
        
        if (!isAllLocationsExists)
        {
            _logger.LogInformation(
                "One or more locations were not found.");
            
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
        await _departmentsRepository.AddAsync(department, cancellationToken);
        
        // логирование
        _logger.LogInformation("Department created with id: {departmentId}", departmentId);

        return departmentId.Value;
    }
}