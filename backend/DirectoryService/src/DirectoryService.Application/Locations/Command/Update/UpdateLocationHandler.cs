using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace DirectoryService.Application.Locations.Command.Update;

public class UpdateLocationHandler : ICommandHandler<Location, UpdateLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<UpdateLocationCommand> _validator;
    private readonly ILogger<UpdateLocationHandler> _logger;
    private readonly HybridCache _cache;

    public UpdateLocationHandler(
        ILocationsRepository locationsRepository,
        ITransactionManager transactionManager,
        IValidator<UpdateLocationCommand> validator,
        ILogger<UpdateLocationHandler> logger,
        HybridCache cache)
    {
        _locationsRepository = locationsRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
        _cache = cache;
    }
    
    public async Task<Result<Location, Errors>> Handle(UpdateLocationCommand command, CancellationToken cancellationToken)
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
        
        // существует ли локация и активна ли она
        var locationResult = await _locationsRepository.GetByIdAsync(
            new LocationId(command.Id), cancellationToken);
        if (locationResult.IsFailure)
        {
            _logger.LogInformation("Location was not found.");
            
            return locationResult.Error.ToErrors();
        }
        
        var location = locationResult.Value;
        
        // подготовка новых данных 
        var updatedNameResult = LocationName.Create(command.UpdateLocationRequest.Name);
        if (updatedNameResult.IsFailure)
        {
            _logger.LogInformation("Failed to update LocationName.");
            
            return updatedNameResult.Error.ToErrors();
        }
        
        var updatedAddressResult = LocationAddress.Create(
            command.UpdateLocationRequest.LocationAddress.Country,
            command.UpdateLocationRequest.LocationAddress.Region,
            command.UpdateLocationRequest.LocationAddress.City,
            command.UpdateLocationRequest.LocationAddress.Street,
            command.UpdateLocationRequest.LocationAddress.House);
        if (updatedAddressResult.IsFailure)
        {
            _logger.LogInformation("Failed to update LocationAddress.");
            
            return updatedAddressResult.Error.ToErrors();
        }
        
        var updatedTimezoneResult = LocationTimezone.Create(command.UpdateLocationRequest.Timezone);
        if (updatedTimezoneResult.IsFailure)
        {
            _logger.LogInformation("Failed to update LocationTimezone.");
            
            return updatedTimezoneResult.Error.ToErrors();
        }
        
        // обновление локации
        var updateLocationsResult = location.Update(updatedNameResult.Value, updatedAddressResult.Value, updatedTimezoneResult.Value);
        if (updateLocationsResult.IsFailure)
        {
            _logger.LogInformation("Failed to update Location.");
            
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
        
        // инвалидация кэша
        await _cache.RemoveByTagAsync(Constants.LOCATION_CACHE_PREFIX, cancellationToken);
        
        return location;
    }
}