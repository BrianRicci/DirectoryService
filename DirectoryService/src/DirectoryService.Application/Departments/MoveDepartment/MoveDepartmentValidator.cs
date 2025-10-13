using DirectoryService.Application.Validation;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Departments.MoveDepartment;

public class MoveDepartmentValidator : AbstractValidator<MoveDepartmentCommand>
{
    public MoveDepartmentValidator()
    {
        RuleFor(command => command.DepartmentId)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("departmentId"));
    }
}