using CSharpFunctionalExtensions;

namespace Shared;

public interface ISoftDeletable
{
    UnitResult<Error> Delete();

    UnitResult<Error> Restore();
}