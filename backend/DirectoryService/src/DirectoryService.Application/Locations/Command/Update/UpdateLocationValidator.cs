using Core.Validation;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Shared.SharedKernel;

namespace DirectoryService.Application.Locations.Command.Update;

public class UpdateLocationValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationValidator()
    {
        RuleFor(command => command.UpdateLocationRequest.Name)
            .MustBeValueObject(LocationName.Create);

        RuleFor(command => command.UpdateLocationRequest.LocationAddress)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("address"))
            .ChildRules(address =>
            {
                address.RuleFor(x => x.Country)
                    .NotEmpty().WithError(GeneralErrors.ValueIsRequired("country"));
                address.RuleFor(x => x.Region)
                    .NotEmpty().WithError(GeneralErrors.ValueIsRequired("region"));
                address.RuleFor(x => x.City)
                    .NotEmpty().WithError(GeneralErrors.ValueIsRequired("city"));
                address.RuleFor(x => x.Street)
                    .NotEmpty().WithError(GeneralErrors.ValueIsRequired("street"));
                address.RuleFor(x => x.House)
                    .NotEmpty().WithError(GeneralErrors.ValueIsRequired("house"));
            });

        RuleFor(command => command.UpdateLocationRequest.Timezone)
            .MustBeValueObject(LocationTimezone.Create);
    }
}