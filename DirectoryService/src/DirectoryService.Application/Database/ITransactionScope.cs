using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Infrastructure.Postgres.Database;

public interface ITransactionScope : IDisposable
{
    UnitResult<Error> Commit();
    
    UnitResult<Error> Rollback();
}