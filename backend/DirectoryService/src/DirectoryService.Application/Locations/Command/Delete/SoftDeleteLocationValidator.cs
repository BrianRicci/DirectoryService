using Core.Validation;
using FluentValidation;
using Shared.SharedKernel;

namespace DirectoryService.Application.Locations.Command.Delete;

public class SoftDeleteLocationValidator : AbstractValidator<SoftDeleteLocationCommand>
{
    public SoftDeleteLocationValidator()
    {
        RuleFor(command => command.LocationId)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("locationId"));
    }
}