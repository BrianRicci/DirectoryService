using Core.Validation;
using DirectoryService.Domain.Positions;
using FluentValidation;

namespace DirectoryService.Application.Positions.Command.Update;

public class UpdatePositionValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionValidator()
    {
        RuleFor(command => command.UpdatePositionRequest.Name)
            .MustBeValueObject(PositionName.Create);

        RuleFor(command => command.UpdatePositionRequest.Description)
            .MustBeValueObject(PositionDescription.Create);
    }
}