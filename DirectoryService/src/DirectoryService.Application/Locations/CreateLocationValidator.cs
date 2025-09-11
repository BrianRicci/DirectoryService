using DirectoryService.Contracts;
using DirectoryService.Domain;
using FluentValidation;

namespace DirectoryService.Application.Locations;

public class CreateLocationValidator : AbstractValidator<CreateLocationDto>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.name)
            .NotEmpty().WithMessage("Название локации не может быть пустым")
            .MinimumLength(LengthConstants.LENGTH3).WithMessage("Название локации слишком короткое")
            .MaximumLength(LengthConstants.LENGTH120).WithMessage("Название локации слишком длинное");
        
        RuleFor(x => x.address)
            .NotEmpty().WithMessage("Адрес локации не может быть пустым");
        
        RuleFor(x => x.timezone)
            .NotEmpty().WithMessage("Часовой пояс локации не может быть пустым");
    }
}