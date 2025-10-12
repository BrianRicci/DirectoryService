using DirectoryService.Application.Validation;
using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using FluentValidation;
using Shared;

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
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("locationIds"))
            .Must(l => l != l.Distinct().ToList()).WithError(GeneralErrors.DuplicateValues("locationIds"));
    }
}