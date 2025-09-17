using FluentValidation.Results;
using Shared;

namespace DirectoryService.Application.Extentions;

public static class ValidationExtensions
{
    public static Errors ToErrors(this ValidationResult validationResult) =>
        validationResult.Errors.Select(e => Error.Validation(
            e.ErrorCode, e.ErrorMessage, e.PropertyName)).ToArray();
}