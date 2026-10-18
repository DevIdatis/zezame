using FluentValidation;
using Zezame.Api.Models.DTOs;

namespace Zezame.Api.Validation {
    public class CreateActivityDtoValidator : AbstractValidator<CreateActivityDto> {
        public CreateActivityDtoValidator() {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
            RuleFor(x => x.ProjectId).NotEmpty().WithMessage("El proyecto es obligatorio.");
            RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
            RuleFor(x => x.EstimatedMinutes).GreaterThan(0)
                .WithMessage("La estimación debe ser mayor que cero.")
                .When(x => x.EstimatedMinutes.HasValue);
        }
    }
}
