namespace backend_trial.Models.DTO.Auth
{
    public class AuthResponseWrapper
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public AuthResponseDto? Data { get; set; }
        public int responseCode { get; set; }
    }
}