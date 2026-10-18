using FluentValidation;
using Zezame.Api.Models.DTOs;

namespace Zezame.Api.Validation {
    public class CreateInvitationDtoValidator : AbstractValidator<CreateInvitationDto> {
        public CreateInvitationDtoValidator() {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(120);
            RuleFor(x => x.Role)
                .Must(r => r is "Employee" or "Manager" or "Admin")
                .WithMessage("Rol no válido. Usa Employee, Manager o Admin.");
        }
    }

    public class AcceptInvitationDtoValidator : AbstractValidator<AcceptInvitationDto> {
        public AcceptInvitationDtoValidator() {
            RuleFor(x => x.Token).NotEmpty();
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(120);
            RuleFor(x => x.Password).Password();
        }
    }
}
