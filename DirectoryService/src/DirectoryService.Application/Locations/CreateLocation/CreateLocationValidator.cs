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
            .NotEmpty().WithMessage("Название локации не может быть пустым")
            .MinimumLength(LengthConstants.LENGTH3).WithMessage("Название локации слишком короткое")
            .MaximumLength(LengthConstants.LENGTH120).WithMessage("Название локации слишком длинное");

        RuleFor(command => command.CreateLocationDto.LocationAddress)
            .NotEmpty().WithMessage("Адрес локации не может быть пустым")
            .ChildRules(address =>
            {
                address.RuleFor(x => x.Country).NotEmpty().WithMessage("Страна не может быть пустой");
                address.RuleFor(x => x.Region).NotEmpty().WithMessage("Регион не может быть пустой");
                address.RuleFor(x => x.City).NotEmpty().WithMessage("Город не может быть пустой");
                address.RuleFor(x => x.Street).NotEmpty().WithMessage("Улица не может быть пустой");
                address.RuleFor(x => x.House).NotEmpty().WithMessage("Номер дома не может быть пустым");
            });

        RuleFor(command => command.CreateLocationDto.Timezone)
            .NotEmpty().WithMessage("Часовой пояс локации не может быть пустым")
            .MustBeValueObject(LocationTimezone.Create);
    }
}