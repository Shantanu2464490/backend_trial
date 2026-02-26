using FluentValidation;
using backend_trial.Models.DTO.Auth;

namespace backend_trial.Validators.Auth
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MinimumLength(3).WithMessage("Minimum 3 characters required.")
                .MaximumLength(50).WithMessage("Maximum 50 characters allowed.")
                .Matches(@"^[a-zA-Z ]*$").WithMessage("Name can only contain letters and spaces.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .MaximumLength(100).WithMessage("Maximum 100 characters allowed.")
                .EmailAddress().WithMessage("Please enter a valid email address (e.g. user@example.com).");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Minimum 8 characters required.")
                .MaximumLength(20).WithMessage("Maximum 20 characters allowed.")
                .Matches(@"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*]).+$")
                    .WithMessage("Password must have at least 1 uppercase letter, 1 number, and 1 special character (!@#$%^&*).");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required.")
                .Must(role => new[] { "Employee", "Manager", "Admin" }.Contains(role))
                    .WithMessage("Role must be Employee, Manager, or Admin.");
        }
    }
}