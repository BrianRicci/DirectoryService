using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain;

public sealed record MediaOwner
{
    private static readonly HashSet<string> _allowedContexts =
    [
        "department",
        "location",
        "position",
    ];

    public string Context { get; }

    public Guid EntityId { get; }

    private MediaOwner(string context, Guid entityId)
    {
        Context = context;
        EntityId = entityId;
    }

    public static Result<MediaOwner, Error> ForDepartment(Guid departmentId) => Create("department", departmentId);
    
    public static Result<MediaOwner, Error> ForLocation(Guid locationId) => Create("location", locationId);
    
    public static Result<MediaOwner, Error> ForPosition(Guid positionId) => Create("position", positionId);
    
    private static Result<MediaOwner, Error> Create(string context, Guid entityId)
    {
        if (string.IsNullOrWhiteSpace(context) || context.Length > 50)
            return GeneralErrors.ValueIsInvalid(nameof(context));

        string normalizeContext = context.Trim().ToLowerInvariant();
        if (!_allowedContexts.Contains(normalizeContext))
            return GeneralErrors.ValueIsInvalid(nameof(context));

        if (entityId == Guid.Empty)
            return GeneralErrors.ValueIsInvalid(nameof(entityId));

        return new MediaOwner(context, entityId);
    }
}