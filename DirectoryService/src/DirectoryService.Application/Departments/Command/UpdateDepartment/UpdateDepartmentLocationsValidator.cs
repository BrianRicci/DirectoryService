using DirectoryService.Application.Validation;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Departments.Command.UpdateDepartment;

public class UpdateDepartmentLocationsValidator : AbstractValidator<UpdateDepartmentLocationsCommand>
{
    public UpdateDepartmentLocationsValidator()
    {
        RuleFor(command => command.UpdateDepartmentLocationsRequest.LocationIds)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("locationIds"))
            .Must(l => l != l.Distinct().ToList()).WithError(GeneralErrors.DuplicateValues("locationIds"));
    }
}