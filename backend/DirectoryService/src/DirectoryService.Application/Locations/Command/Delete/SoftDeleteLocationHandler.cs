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

namespace DirectoryService.Application.Locations.Command.Delete;

public class SoftDeleteLocationHandler : ICommandHandler<Guid, SoftDeleteLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<SoftDeleteLocationCommand> _validator;
    private readonly ILogger<SoftDeleteLocationHandler> _logger;
    private readonly HybridCache _cache;

    public SoftDeleteLocationHandler(
        ILocationsRepository locationsRepository,
        ITransactionManager transactionManager,
        IValidator<SoftDeleteLocationCommand> validator,
        ILogger<SoftDeleteLocationHandler> logger, 
        HybridCache cache)
    {
        _locationsRepository = locationsRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
        _cache = cache;
    }
    
    public async Task<Result<Guid, Errors>> Handle(SoftDeleteLocationCommand command, CancellationToken cancellationToken)
    {
        // валидация
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToList();
        }
        
        // открытие транзакции
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            _logger.LogInformation("Failed to begin transaction.");
            return transactionScopeResult.Error.ToErrors();
        }
        
        using var transactionScope = transactionScopeResult.Value;

        var locationId = new LocationId(command.LocationId);

        var locationResult =
            await _locationsRepository.GetByIdWithLock(locationId, cancellationToken);
        if (locationResult.IsFailure)
        {
            _logger.LogInformation("Location was not found.");
            transactionScope.Rollback();
            return locationResult.Error.ToErrors();
        }
        
        // TODO
        // Ограничение, для запрета удалять локацию, если у нее есть связь с департаментом
        // решил пока без ограничения сделать, так как для него придется менять еще логику в хэндлерах департаментов
        
        // var relatedDepartmentsCountResult =
        //     await _locationsRepository.GetRelatedDepartmentsAsync(locationId, cancellationToken);
        // if (relatedDepartmentsCountResult.IsFailure)
        // {
        //     _logger.LogInformation("Failed to get related departments.");
        //     transactionScope.Rollback();
        //     return relatedDepartmentsCountResult.Error.ToErrors();
        // }
        //
        // if (relatedDepartmentsCountResult.Value > 0)
        // {
        //     _logger.LogInformation("Location has related departments.");
        //     transactionScope.Rollback();
        //     return GeneralErrors.NotFound(locationId.Value).ToErrors();
        // }
        var location = locationResult.Value;
        
        location.SoftDelete();
        
        var saveChanges = await _locationsRepository.SaveChangesAsync(cancellationToken);
        if (saveChanges.IsFailure)
        {
            transactionScope.Rollback();
            return saveChanges.Error.ToErrors();
        }
        
        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            _logger.LogInformation("Failed to commit transaction.");
            transactionScope.Rollback();
            return commitResult.Error.ToErrors();
        }
        
        // инвалидация кэша
        await _cache.RemoveByTagAsync(Constants.LOCATION_CACHE_PREFIX, cancellationToken);
        
        _logger.LogInformation("Location {LocationId} softly deleted", command.LocationId);
        
        return location.Id.Value;
    }
}