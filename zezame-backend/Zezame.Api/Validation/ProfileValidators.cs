using FluentValidation;
using Zezame.Api.Models.DTOs;

namespace Zezame.Api.Validation {
    public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto> {
        public UpdateProfileDtoValidator() {
            // Actualización parcial de perfil: validar solo los campos presentes.
            RuleFor(x => x.Email).EmailAddress().MaximumLength(120)
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(120)
                .When(x => x.FullName is not null);
        }
    }
}
