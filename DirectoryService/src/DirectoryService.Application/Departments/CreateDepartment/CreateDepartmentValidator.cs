using DirectoryService.Application.Validation;
using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using FluentValidation;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentValidator: AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentValidator()
    {
        RuleFor(command => command.CreateDepartmentRequest.Name)
            .MustBeValueObject(DepartmentName.Create);
        
        RuleFor(command => command.CreateDepartmentRequest.Identifier)
            .MustBeValueObject(DepartmentIdentifier.Create);

        RuleFor(command => command.CreateDepartmentRequest.LocationIds)
            .NotEmpty().WithMessage("Массив локаций не может быть пустым")
            .Must(l => l != l.Distinct().ToList()).WithMessage("Массив локаций содержит дублирующиеся значения");
    }
}