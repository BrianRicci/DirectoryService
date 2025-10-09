using FluentValidation;

namespace DirectoryService.Application.Departments.MoveDepartment;

public class MoveDepartmentValidator : AbstractValidator<MoveDepartmentCommand>
{
    public MoveDepartmentValidator()
    {
        RuleFor(command => command.DepartmentId)
            .NotEmpty().WithMessage("DepartmentId обязательно");
    }
}