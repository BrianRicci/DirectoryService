using DirectoryService.Application.Validation;
using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using FluentValidation;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentValidator: AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentValidator()
    {
        RuleFor(command => command.CreateDepartmentDto.Name)
            .MustBeValueObject(DepartmentName.Create);
        
        RuleFor(command => command.CreateDepartmentDto.Identifier)
            .MustBeValueObject(DepartmentIdentifier.Create);

        RuleFor(command => command.CreateDepartmentDto.LocationIds)
            .NotEmpty().WithMessage("Массив локаций не может быть пустым")
            .Must(l => l != l.Distinct().ToList()).WithMessage("Массив локаций содержит дублирующиеся значения");
    }
}