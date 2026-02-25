using System.ComponentModel.DataAnnotations;

namespace backend_trial.Models.DTO.Auth
{
    public class LoginRequestDto
    {
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
    }
}
