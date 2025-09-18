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
            return validationResult.ToErrors();
        }
        
        // создание сущности локации
        var locationNameResult = LocationName.Create(command.CreateLocationDto.Name);
        
        var locationAddressResult = LocationAddress.Create(
            command.CreateLocationDto.Address.Country,
            command.CreateLocationDto.Address.Region,
            command.CreateLocationDto.Address.City,
            command.CreateLocationDto.Address.Street,
            command.CreateLocationDto.Address.House);
        
        var locationTimezoneResult = LocationTimezone.Create(command.CreateLocationDto.Timezone);
        
        var locationResult = Location.Create(
            locationNameResult.Value,
            locationAddressResult.Value,
            locationTimezoneResult.Value);

        // сохранение сущности локации в БД
        await _locationsRepository.AddAsync(locationResult.Value, cancellationToken);
        
        // логирование
        LocationId locationId = locationResult.Value.Id;
        
        _logger.LogInformation("Location created with id: {locationId}", locationId);

        return locationId.Value;
    }
}