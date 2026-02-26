namespace backend_trial.Models.DTO.Auth
{
    public class RegisterRequestDto
    {
        public string Name { get; set; } = null;
        public string Email { get; set; } = null;
        public string Password { get; set; } = null;
        public string Role { get; set; } = null;
    }
}