using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Extentions;
using DirectoryService.Application.Locations.Fails.Exceptions;
using DirectoryService.Contracts;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateLocationDto> _validator;
    private readonly ILogger<CreateLocationHandler> _logger;
    
    public CreateLocationHandler(
        ILocationsRepository locationsRepository,
        IValidator<CreateLocationDto> validator,
        ILogger<CreateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Failure>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        // валидация
        var validationResult = await _validator.ValidateAsync(command.CreateLocationDto, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            throw new LocationValidationException(validationResult.ToErrors());
        }
        
        // создание сущности локации
        Result<Location> locationResult = Location.Create(
            command.CreateLocationDto.name,
            command.CreateLocationDto.address.Country,
            command.CreateLocationDto.address.Region,
            command.CreateLocationDto.address.City,
            command.CreateLocationDto.address.Street,
            command.CreateLocationDto.address.House,
            command.CreateLocationDto.timezone);

        // сохранение сущности локации в БД
        await _locationsRepository.AddAsync(locationResult.Value, cancellationToken);
        
        // логирование
        LocationId locationId = locationResult.Value.Id;
        
        _logger.LogInformation("Location created with id: {locationId}", locationId);

        return locationId.Value;
    }
}