using Core.Validation;
using FluentValidation;
using Shared.SharedKernel;

namespace DirectoryService.Application.Positions.Command.SoftDelete;

public class SoftDeletePositionValidator : AbstractValidator<SoftDeletePositionCommand>
{
    public SoftDeletePositionValidator()
    {
        RuleFor(command => command.PositionId)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("positionId"));
    }
}