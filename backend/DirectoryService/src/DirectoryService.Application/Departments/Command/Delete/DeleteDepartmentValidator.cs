using Core.Validation;
using FluentValidation;
using Shared.SharedKernel;

namespace DirectoryService.Application.Departments.Command.Delete;

public class DeleteDepartmentValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentValidator()
    {
        RuleFor(command => command.DepartmentId)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("departmentId"));
    }
}