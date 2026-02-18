namespace backend_trial.Models.DTO
{
    public class AuthResponseWrapper
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public AuthResponseDto? Data { get; set; }
    }
}