using DirectoryService.Contracts;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations;

public class LocationsService : ILocationsService
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateLocationDto> _validator;
    private readonly ILogger<LocationsService> _logger;
    
    public LocationsService(
        ILocationsRepository locationsRepository,
        IValidator<CreateLocationDto> validator,
        ILogger<LocationsService> logger)
    {
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }
    
    public async Task<Guid> Create(CreateLocationDto locationDto, CancellationToken cancellationToken)
    {
        // валидация
        var validationResult = await _validator.ValidateAsync(locationDto, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        
        // создание сущности локации
        var locationId = new LocationId(Guid.NewGuid());
        
        var locationName = new LocationName(locationDto.name);
        
        var locationAddress = new LocationAddress(
            locationDto.address.Country,
            locationDto.address.Region,
            locationDto.address.City,
            locationDto.address.Street,
            locationDto.address.House);
        
        var locationTimezone = new LocationTimezone(locationDto.timezone);
        
        var location = new Location(
            locationId,
            locationName,
            locationAddress,
            locationTimezone);

        // сохранение сущности локации в БД
        await _locationsRepository.AddAsync(location, cancellationToken);
        
        // логирование
        _logger.LogInformation("Location created with id: {LocationId}", locationId);

        return locationId.Value;
    }
}