using DirectoryService.Contracts;
using DirectoryService.Domain;
using FluentValidation;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationValidator : AbstractValidator<CreateLocationDto>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название локации не может быть пустым")
            .MinimumLength(LengthConstants.LENGTH3).WithMessage("Название локации слишком короткое")
            .MaximumLength(LengthConstants.LENGTH120).WithMessage("Название локации слишком длинное");
        
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Адрес локации не может быть пустым");
        
        RuleFor(x => x.Timezone)
            .NotEmpty().WithMessage("Часовой пояс локации не может быть пустым");
    }
}