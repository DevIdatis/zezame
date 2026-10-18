using FluentValidation;
using Zezame.Api.Models.DTOs;
using static Zezame.Api.Models.AuthDtos;

namespace Zezame.Api.Validation {
    /// <summary>Política de contraseña reutilizable entre registro y cambio de contraseña.</summary>
    internal static class PasswordRules {
        public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> rule) =>
            rule.NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
                .Matches("[A-Z]").WithMessage("La contraseña debe incluir al menos una mayúscula.")
                .Matches("[a-z]").WithMessage("La contraseña debe incluir al menos una minúscula.")
                .Matches("[0-9]").WithMessage("La contraseña debe incluir al menos un número.");
    }

    public class RegisterDtoValidator : AbstractValidator<RegisterDto> {
        public RegisterDtoValidator() {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(120);
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(120);
            RuleFor(x => x.Password).Password();
        }
    }

    public class LoginDtoValidator : AbstractValidator<LoginDto> {
        public LoginDtoValidator() {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto> {
        public ChangePasswordDtoValidator() {
            RuleFor(x => x.CurrentPassword).NotEmpty();
            RuleFor(x => x.NewPassword).Password();
        }
    }
}
