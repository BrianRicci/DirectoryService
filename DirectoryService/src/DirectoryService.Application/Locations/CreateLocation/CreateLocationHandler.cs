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
        
        // создание сущности
        var locationName = LocationName.Create(command.CreateLocationRequest.Name).Value;
        
        var locationAddress = LocationAddress.Create(
            command.CreateLocationRequest.LocationAddress.Country,
            command.CreateLocationRequest.LocationAddress.Region,
            command.CreateLocationRequest.LocationAddress.City,
            command.CreateLocationRequest.LocationAddress.Street,
            command.CreateLocationRequest.LocationAddress.House).Value;
        
        bool isAddressExists = await _locationsRepository.IsAddressExistsAsync(locationAddress, cancellationToken);
        
        if (isAddressExists)
        {
            _logger.LogInformation(
                "Location with this address already exists.");
            
            return GeneralErrors.ValueAlreadyExists("address").ToErrors();
        }
        
        var locationTimezone = LocationTimezone.Create(command.CreateLocationRequest.Timezone).Value;
        
        var locationId = new LocationId(Guid.NewGuid());
        
        var location = Location.Create(
            locationId,
            locationName,
            locationAddress,
            locationTimezone).Value;

        // сохранение сущности в БД
        var savedLocationResult = await _locationsRepository.AddAsync(location, cancellationToken);
        
        // логирование
        _logger.LogInformation("Location created with id: {locationId}", location.Id.Value);

        return savedLocationResult;
    }
}