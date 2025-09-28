using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Positions.CreatePosition;

public class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidator()
    {
        RuleFor(command => command.CreatePositionRequest.Name)
            .MustBeValueObject(PositionName.Create);

        RuleFor(command => command.CreatePositionRequest.Description)
            .MustBeValueObject(PositionDescription.Create);

        RuleFor(command => command.CreatePositionRequest.DepartmentIds)
            .NotEmpty().WithMessage("Массив подразделений не может быть пустым")
            .Must(l => l != l.Distinct().ToList()).WithMessage("Массив подразделений содержит дублирующиеся значения");
    }
}