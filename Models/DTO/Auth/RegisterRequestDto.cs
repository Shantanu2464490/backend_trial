using System.ComponentModel.DataAnnotations;

namespace backend_trial.Models.DTO.Auth
{
    public class RegisterRequestDto
    {
        // ─── NAME ───────────────────────────────────────────────
        [Required(ErrorMessage = "Name is required.")]
        [MinLength(3, ErrorMessage = "Minimum 3 characters required.")]
        [MaxLength(50, ErrorMessage = "Maximum 50 characters allowed.")]
        [RegularExpression(@"^[a-zA-Z ]*$",
            ErrorMessage = "Name can only contain letters and spaces.")]
        public string Name { get; set; } = null;

        // ─── EMAIL ──────────────────────────────────────────────
        [Required(ErrorMessage = "Email is required.")]
        [MaxLength(100, ErrorMessage = "Maximum 100 characters allowed.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Please enter a valid email address (e.g. user@example.com).")]
        public string Email { get; set; } = null;

        // ─── PASSWORD ───────────────────────────────────────────
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Minimum 8 characters required.")]
        [MaxLength(20, ErrorMessage = "Maximum 20 characters allowed.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*]).+$",
            ErrorMessage = "Password must have at least 1 uppercase letter, 1 number, and 1 special character (!@#$%^&*).")]
        public string Password { get; set; } = null;

        // ─── ROLE ───────────────────────────────────────────────
        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression(@"^(Employee|Manager|Admin)$",
            ErrorMessage = "Role must be Employee, Manager, or Admin.")]
        public string Role { get; set; } = null;
    }
}
