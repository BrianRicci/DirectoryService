using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace DirectoryService.Application.Positions.Command.Create;

public class CreatePositionHandler : ICommandHandler<Guid, CreatePositionCommand>
{
    private const char SEPARATOR = '.';
    private readonly IPositionsRepository _positionsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly ILogger<CreatePositionHandler> _logger;

    public CreatePositionHandler(
        IPositionsRepository positionsRepository,
        IDepartmentsRepository departmentsRepository,
        IValidator<CreatePositionCommand> validator,
        ILogger<CreatePositionHandler> logger)
    {
        _positionsRepository = positionsRepository;
        _departmentsRepository = departmentsRepository;
        _validator = validator;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Errors>> Handle(CreatePositionCommand command, CancellationToken cancellationToken)
    {
        // валидация
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            return validationResult.ToList();
        }
        
        // создание сущности
        var positionName = PositionName.Create(command.CreatePositionRequest.Name).Value;
        
        var isNameExists = await _positionsRepository.IsNameExistsAsync(positionName, cancellationToken);
        
        if (isNameExists)
        {
            _logger.LogInformation(
                "Position with this name already exists.");
            
            return GeneralErrors.ValueAlreadyExists("name").ToErrors();
        }
        
        var positionDescription = PositionDescription.Create(
            command.CreatePositionRequest.Description).Value;

        var positionDepartmentIds = command.CreatePositionRequest.DepartmentIds
            .Select(departmentId => new DepartmentId(departmentId)).ToList();
        
        bool isAllDepartmentsExistsAndActive = await _departmentsRepository
            .IsAllExistsAsync(positionDepartmentIds, cancellationToken);
        
        if (!isAllDepartmentsExistsAndActive)
        {
            _logger.LogInformation(
                "One or more departments were not found or are not active.");

            return Error.NotFound(
                "departments.not.found", 
                "Одна или несколько подразделений не найдены или не активны").ToErrors();
        }
        
        var positionId = new PositionId(Guid.NewGuid());
        
        var departmentPositions = positionDepartmentIds 
            .Select(departmentId => new DepartmentPosition(departmentId, positionId)).ToList();
        
        var position = Position.Create(
            positionId,
            positionName,
            positionDescription,
            departmentPositions).Value;
        
        // сохранение сущности в БД
        var savedPositionResult = await _positionsRepository.AddAsync(position, cancellationToken);
        if (savedPositionResult.IsFailure)
        {
            _logger.LogInformation("Failed to save position.");
            return savedPositionResult.Error.ToErrors();
        }
        
        // логирование
        _logger.LogInformation("Position created with id: {positionId}", position.Id.Value);

        return savedPositionResult.Value;
    }
}