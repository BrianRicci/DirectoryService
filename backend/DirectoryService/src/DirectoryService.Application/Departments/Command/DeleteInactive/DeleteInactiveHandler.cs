using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace DirectoryService.Application.Departments.Command.DeleteInactive;

public class DeleteInactiveHandler
{
    private readonly ILogger<DeleteInactiveHandler> _logger;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IPositionsRepository _positionsRepository;
    private readonly ITransactionManager _transactionManager;

    public DeleteInactiveHandler(
        ILogger<DeleteInactiveHandler> logger,
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IPositionsRepository positionsRepository,
        ITransactionManager transactionManager)
    {
        _logger = logger;
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _positionsRepository = positionsRepository;
        _transactionManager = transactionManager;
    }
    
    public async Task<UnitResult<Errors>> Handle(CancellationToken cancellationToken)
    {
        // открыть транзакцию
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            _logger.LogInformation("Failed to begin transaction.");
            return transactionScopeResult.Error.ToErrors();
        }
        
        using var transactionScope = transactionScopeResult.Value;
        
        FilterOptions timeFilterOptions = FilterOptions.FromMonthsAgo(1);
        
        // получение неактивных больше месяца назад департаментов
        var inactiveDepartmentsResult = await _departmentsRepository.GetInactiveAsync(timeFilterOptions, cancellationToken);
        if (inactiveDepartmentsResult.IsFailure)
        {
            _logger.LogInformation("Failed to get inactive departments.");
            return inactiveDepartmentsResult.Error.ToErrors();
        }

        var inactiveDepartments = inactiveDepartmentsResult.Value;

        // TODO
        // добавить здесь проверку на наличие неактивных департаментов, если их нет - выход из хэндлера
        // далее и с родительскими так
        
        // получение дочерних элементов первого колена неактивных департаментов
        var firstDescendantsOfInactiveDepartmentsResult = await _departmentsRepository.GetByParentIdsAsync(
            inactiveDepartments.Select(d => d.Id).ToList(), cancellationToken);
        if (firstDescendantsOfInactiveDepartmentsResult.IsFailure)
        {
            _logger.LogInformation("Failed to get first descendants of inactive departments.");
            return firstDescendantsOfInactiveDepartmentsResult.Error.ToErrors();
        }

        // получение родительских департаментов неактивных департаментов
        var parentsOfInactiveDepartmentsResult = await _departmentsRepository.GetByIdsAsync(
            inactiveDepartments.Select(d => d.ParentId).ToList(), cancellationToken);
        if (parentsOfInactiveDepartmentsResult.IsFailure)
        {
            _logger.LogInformation("Failed to get parents of inactive departments.");
            return parentsOfInactiveDepartmentsResult.Error.ToErrors();
        }
        
        var parentsOfInactiveDepartments = parentsOfInactiveDepartmentsResult.Value;

        List<Department> firstDescendantsOfInactiveDepartments = firstDescendantsOfInactiveDepartmentsResult.Value;

        foreach (var descendant in firstDescendantsOfInactiveDepartments)
        {
            Department? oldParent = inactiveDepartments.FirstOrDefault(d => d.Id == descendant.ParentId);
            Department? newParent = parentsOfInactiveDepartments.FirstOrDefault(d => d.Id == oldParent?.ParentId);
            var oldPath = descendant.Path;
            int depthDelta = descendant.SetParent(newParent);
            
            var updateDescendantDepartmentsResult = await _departmentsRepository.BulkUpdateDescendantsPath(
                oldPath,
                descendant.Path,
                depthDelta,
                cancellationToken);
            if (updateDescendantDepartmentsResult.IsFailure)
            {
                _logger.LogInformation("Failed to update descendant departments.");
                transactionScope.Rollback();
                return updateDescendantDepartmentsResult.Error.ToErrors();
            }
        }
        
        var saveChanges = await _departmentsRepository.SaveChangesAsync(cancellationToken);
        if (saveChanges.IsFailure)
        {
            _logger.LogInformation("Failed to save changes.");
            transactionScope.Rollback();
            return saveChanges.Error.ToErrors();
        }
        
        // удаление неактивных элементов
        // с департаментом каскадно удалятся связи department_locations, department_positions
        await _departmentsRepository.BulkDeleteAsync(
            inactiveDepartments.Select(d => d.Id).ToList(), cancellationToken);
        
        // очистка оставшихся элементов
        await _locationsRepository.DeleteInactiveAsync(cancellationToken);
        await _positionsRepository.DeleteInactiveAsync(cancellationToken);
        
        // закрыть транзакцию
        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            _logger.LogInformation("Failed to commit transaction.");
            transactionScope.Rollback();
            return commitResult.Error.ToErrors();
        }
        
        return UnitResult.Success<Errors>();
    }
}