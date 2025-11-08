using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain.Departments;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments.Command.DeleteInactive;

public class DeleteInactiveHandler
{
    private readonly ILogger<DeleteInactiveHandler> _logger;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ITransactionManager _transactionManager;

    public DeleteInactiveHandler(
        ILogger<DeleteInactiveHandler> logger,
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager)
    {
        _logger = logger;
        _departmentsRepository = departmentsRepository;
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
        
        // удаление неактивных департаментов, его связей и связанных локаций и позиций
        await _departmentsRepository.BulkDeleteAsync(
            inactiveDepartments.Select(d => d.Id).ToList(), cancellationToken);
        
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