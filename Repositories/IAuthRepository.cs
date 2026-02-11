using backend_trial.Models.DTO;

namespace backend_trial.Repositories
{
    public interface IAuthRepository
    {
        Task<bool> UserExistsAsync(string email);
        Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto request);
        Task<(bool Success, AuthResponseDto? User, string Message)> LoginAsync(LoginRequestDto request);
    }
}
