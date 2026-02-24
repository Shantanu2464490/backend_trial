using backend_trial.Models.DTO.Auth;

namespace backend_trial.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<bool> UserExistsAsync(string email);
        Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto request);
        Task<(bool Success, AuthResponseDto? User, string Message)> LoginAsync(LoginRequestDto request);
    }
}
