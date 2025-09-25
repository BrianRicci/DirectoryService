using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Extentions;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateLocationCommand> _validator;
    private readonly ILogger<CreateLocationHandler> _logger;
    
    public CreateLocationHandler(
        ILocationsRepository locationsRepository,
        IValidator<CreateLocationCommand> validator,
        ILogger<CreateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        // валидация
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            return validationResult.ToList();
        }
        
        // создание сущности локации
        var locationName = LocationName.Create(command.CreateLocationDto.Name).Value;
        
        var locationAddress = LocationAddress.Create(
            command.CreateLocationDto.LocationAddress.Country,
            command.CreateLocationDto.LocationAddress.Region,
            command.CreateLocationDto.LocationAddress.City,
            command.CreateLocationDto.LocationAddress.Street,
            command.CreateLocationDto.LocationAddress.House).Value;
        
        bool isAddressExists = await _locationsRepository.IsAddressExistsAsync(locationAddress, cancellationToken);
        
        if (isAddressExists)
        {
            _logger.LogInformation(
                "Location with this address already exists.");
            
            return GeneralErrors.ValueAlreadyExists("address").ToErrors();
        }
        
        var locationTimezone = LocationTimezone.Create(command.CreateLocationDto.Timezone).Value;
        
        var location = Location.Create(
            locationName,
            locationAddress,
            locationTimezone).Value;

        // сохранение сущности локации в БД
        await _locationsRepository.AddAsync(location, cancellationToken);
        
        // логирование
        LocationId locationId = location.Id;
        
        _logger.LogInformation("Location created with id: {locationId}", locationId);

        return locationId.Value;
    }
}