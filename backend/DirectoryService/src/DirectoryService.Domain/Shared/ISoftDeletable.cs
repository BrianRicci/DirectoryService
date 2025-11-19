using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace DirectoryService.Domain.Shared;

public interface ISoftDeletable
{
    UnitResult<Error> Delete();

    UnitResult<Error> Restore();
}