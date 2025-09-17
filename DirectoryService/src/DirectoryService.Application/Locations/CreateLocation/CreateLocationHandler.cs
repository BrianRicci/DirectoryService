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
    
    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        // валидация
        var validationResult = await _validator.ValidateAsync(command.CreateLocationDto, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }
        
        // создание сущности локации
        LocationName locationName = LocationName.Create(command.CreateLocationDto.Name).Value;
        LocationAddress locationAddress = LocationAddress.Create(
            command.CreateLocationDto.Address.Country,
            command.CreateLocationDto.Address.Region,
            command.CreateLocationDto.Address.City,
            command.CreateLocationDto.Address.Street,
            command.CreateLocationDto.Address.House).Value;
        LocationTimezone locationTimezone = LocationTimezone.Create(command.CreateLocationDto.Timezone).Value;
        
        Result<Location> locationResult = Location.Create(locationName, locationAddress, locationTimezone);

        // сохранение сущности локации в БД
        await _locationsRepository.AddAsync(locationResult.Value, cancellationToken);
        
        // логирование
        LocationId locationId = locationResult.Value.Id;
        
        _logger.LogInformation("Location created with id: {locationId}", locationId);

        return locationId.Value;
    }
}