using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace DirectoryService.Application.Positions.Command.SoftDelete;

public class SoftDeletePositionHandler
{
    private readonly IPositionsRepository _positionsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<SoftDeletePositionCommand> _validator;
    private readonly ILogger<SoftDeletePositionHandler> _logger;
    private readonly HybridCache _cache;

    public SoftDeletePositionHandler(
        IPositionsRepository positionsRepository,
        ITransactionManager transactionManager,
        IValidator<SoftDeletePositionCommand> validator,
        ILogger<SoftDeletePositionHandler> logger,
        HybridCache cache)
    {
        _positionsRepository = positionsRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
        _cache = cache;
    }
    
    public async Task<Result<Guid, Errors>> Handle(
        SoftDeletePositionCommand command, CancellationToken cancellationToken)
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

        var positionId = new PositionId(command.PositionId);

        var positionResult =
            await _positionsRepository.GetByIdWithLock(positionId, cancellationToken);
        if (positionResult.IsFailure)
        {
            _logger.LogInformation("Position was not found.");
            transactionScope.Rollback();
            return positionResult.Error.ToErrors();
        }

        // TODO
        // Ограничение, для запрета удалять позиции, если у нее есть связь с департаментом
        // решил пока без ограничения сделать, так как для него придется менять еще логику в хэндлерах департаментов
        // смотреть примерную реализцию в SoftDeleteLocationHandler
       
        var position = positionResult.Value;
        
        position.SoftDelete();
        
        var saveChanges = await _positionsRepository.SaveChangesAsync(cancellationToken);
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
        await _cache.RemoveByTagAsync(Constants.POSITION_CACHE_PREFIX, cancellationToken);
        
        _logger.LogInformation("Position {PositionId} softly deleted", command.PositionId);
        
        return position.Id.Value;
    }
}