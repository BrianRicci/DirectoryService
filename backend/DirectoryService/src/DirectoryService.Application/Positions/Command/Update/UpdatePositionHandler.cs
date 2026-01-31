using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace DirectoryService.Application.Positions.Command.Update;

public class UpdatePositionHandler: ICommandHandler<Position, UpdatePositionCommand>
{
    private readonly IPositionsRepository _positionsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<UpdatePositionCommand> _validator;
    private readonly ILogger<UpdatePositionHandler> _logger;
    private readonly HybridCache _cache;

    public UpdatePositionHandler(
        IPositionsRepository positionsRepository,
        ITransactionManager transactionManager,
        IValidator<UpdatePositionCommand> validator,
        ILogger<UpdatePositionHandler> logger,
        HybridCache cache)
    {
        _positionsRepository = positionsRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
        _cache = cache;
    }
    
    public async Task<Result<Position, Errors>> Handle(UpdatePositionCommand command, CancellationToken cancellationToken)
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
        
        // существует ли позиция и активна ли она
        var positionResult = await _positionsRepository.GetByIdAsync(
            new PositionId(command.Id), cancellationToken);
        if (positionResult.IsFailure)
        {
            _logger.LogInformation("Position was not found.");
            
            return positionResult.Error.ToErrors();
        }
        
        var position = positionResult.Value;
        
        // подготовка новых данных 
        var updatedNameResult = PositionName.Create(command.UpdatePositionRequest.Name);
        if (updatedNameResult.IsFailure)
        {
            _logger.LogInformation("Failed to update PositionName.");
            
            return updatedNameResult.Error.ToErrors();
        }
        
        var updatedDescriptionResult = PositionDescription.Create(command.UpdatePositionRequest.Description);
        if (updatedDescriptionResult.IsFailure)
        {
            _logger.LogInformation("Failed to update PositionDescription.");
            
            return updatedDescriptionResult.Error.ToErrors();
        }
        
        // обновление позиции
        var updatePositionResult = position.Update(updatedNameResult.Value, updatedDescriptionResult.Value);
        if (updatePositionResult.IsFailure)
        {
            _logger.LogInformation("Failed to update position.");
            
            return updatePositionResult.Error.ToErrors();
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
        await _cache.RemoveByTagAsync(Constants.POSITION_CACHE_PREFIX, cancellationToken);
        
        return position;
    }
}