using backend_trial.Models.Domain;

namespace backend_trial.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateJwtToken(User user);
    }
}
