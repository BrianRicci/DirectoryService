using Core.Validation;
using FluentValidation;
using Shared.SharedKernel;

namespace DirectoryService.Application.Departments.Command.Move;

public class MoveDepartmentValidator : AbstractValidator<MoveDepartmentCommand>
{
    public MoveDepartmentValidator()
    {
        RuleFor(command => command.DepartmentId)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("departmentId"));
    }
}