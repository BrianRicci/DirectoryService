using FluentValidation;

namespace DirectoryService.Application.Departments.UpdateDepartment;

public class UpdateDepartmentLocationsValidator : AbstractValidator<UpdateDepartmentLocationsCommand>
{
    public UpdateDepartmentLocationsValidator()
    {
        RuleFor(command => command.UpdateDepartmentLocationsRequest.LocationIds)
            .NotEmpty().WithMessage("Массив локаций не может быть пустым")
            .Must(l => l != l.Distinct().ToList()).WithMessage("Массив локаций содержит дублирующиеся значения");
    }
}