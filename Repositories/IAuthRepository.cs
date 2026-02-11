using backend_trial.Models.DTO;

namespace backend_trial.Repositories
{
    public class IAuthRepository
    {
        Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
    }
}
