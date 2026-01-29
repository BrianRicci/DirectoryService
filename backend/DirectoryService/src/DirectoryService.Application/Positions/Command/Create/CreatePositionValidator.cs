using Core.Validation;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Shared.SharedKernel;

namespace DirectoryService.Application.Positions.Command.Create;

public class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidator()
    {
        RuleFor(command => command.CreatePositionRequest.Name)
            .MustBeValueObject(PositionName.Create);

        RuleFor(command => command.CreatePositionRequest.Description)
            .MustBeValueObject(PositionDescription.Create);

        RuleFor(command => command.CreatePositionRequest.DepartmentIds)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("departmentIds"))
            .Must(l => l != l.Distinct().ToList()).WithError(GeneralErrors.DuplicateValues("departmentIds"));
    }
}