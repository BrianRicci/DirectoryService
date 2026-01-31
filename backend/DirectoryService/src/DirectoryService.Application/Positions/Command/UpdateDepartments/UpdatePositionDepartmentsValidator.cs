using Core.Validation;
using FluentValidation;
using Shared.SharedKernel;

namespace DirectoryService.Application.Positions.Command.UpdateDepartments;

public class UpdatePositionDepartmentsValidator : AbstractValidator<UpdatePositionDepartmentsCommand>
{
    public UpdatePositionDepartmentsValidator()
    {
        RuleFor(command => command.UpdatePositionDepartmentsRequest.DepartmentIds)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("departmentIds"))
            .Must(l => l != l.Distinct().ToList()).WithError(GeneralErrors.DuplicateValues("departmentIds"));
    }
}