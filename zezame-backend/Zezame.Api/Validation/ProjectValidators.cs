using FluentValidation;
using Zezame.Api.Models.DTOs;

namespace Zezame.Api.Validation {
    public class CreateProjectDtoValidator : AbstractValidator<CreateProjectDto> {
        public CreateProjectDtoValidator() {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
            RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
        }
    }

    public class UpdateProjectDtoValidator : AbstractValidator<UpdateProjectDto> {
        public UpdateProjectDtoValidator() {
            // Todos los campos son opcionales (actualización parcial); se validan solo si vienen.
            RuleFor(x => x.Name).NotEmpty().MaximumLength(120).When(x => x.Name is not null);
            RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
            RuleFor(x => x.Status)
                .Must(s => s is "Active" or "Paused" or "Completed")
                .WithMessage("Estado no válido. Usa Active, Paused o Completed.")
                .When(x => x.Status is not null);
        }
    }
}
