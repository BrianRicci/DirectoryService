using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace DirectoryService.Domain.Shared;

public interface ISoftDeletable
{
    UnitResult<Error> SoftDelete();

    UnitResult<Error> Restore();
}