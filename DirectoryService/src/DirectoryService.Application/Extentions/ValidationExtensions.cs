using FluentValidation.Results;
using Shared;

namespace DirectoryService.Application.Extentions;

public static class ValidationExtensions
{
    public static Errors ToList(this ValidationResult validationResult)
    {
        var validationErrors = validationResult.Errors;
        
        // var errors = validationErrors
        //     .Select(e =>
        //     {
        //         var error = Error.Deserialize(e.ErrorMessage);
        //         
        //         return Error.Validation(
        //             error.Code,
        //             error.Message,
        //             e.PropertyName);
        //     }).ToList();
        
        var errors = from validationError in validationErrors
            let errorMessage = validationError.ErrorMessage
            let error = Error.Deserialize(errorMessage)
            select Error.Validation(error.Code, error.Message, validationError.PropertyName);
        
        return new Errors(errors.ToList());
    }
}