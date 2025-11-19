using Core.Validation;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Shared.SharedKernel;

namespace DirectoryService.Application.Locations.Command.Create;

public class CreateLocationValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationValidator()
    {
        RuleFor(command => command.CreateLocationRequest.Name)
            .MustBeValueObject(LocationName.Create);

        RuleFor(command => command.CreateLocationRequest.LocationAddress)
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

        RuleFor(command => command.CreateLocationRequest.Timezone)
            .MustBeValueObject(LocationTimezone.Create);
    }
}