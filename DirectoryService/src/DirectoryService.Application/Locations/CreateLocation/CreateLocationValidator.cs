using DirectoryService.Application.Validation;
using DirectoryService.Contracts;
using DirectoryService.Domain;
using DirectoryService.Domain.Locations;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationValidator()
    {
        RuleFor(command => command.CreateLocationDto.Name)
            .MustBeValueObject(LocationName.Create);

        RuleFor(command => command.CreateLocationDto.LocationAddress)
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

        RuleFor(command => command.CreateLocationDto.Timezone)
            .MustBeValueObject(LocationTimezone.Create);
    }
}