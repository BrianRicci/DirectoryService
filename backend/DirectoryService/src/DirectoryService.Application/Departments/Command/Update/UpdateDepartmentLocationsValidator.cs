using Core.Validation;
using FluentValidation;
using Shared.SharedKernel;

namespace DirectoryService.Application.Departments.Command.Update;

public class UpdateDepartmentLocationsValidator : AbstractValidator<UpdateDepartmentLocationsCommand>
{
    public UpdateDepartmentLocationsValidator()
    {
        RuleFor(command => command.UpdateDepartmentLocationsRequest.LocationIds)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("locationIds"))
            .Must(l => l != l.Distinct().ToList()).WithError(GeneralErrors.DuplicateValues("locationIds"));
    }
}