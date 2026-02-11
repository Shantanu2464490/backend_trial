using backend_trial.Models.DTO;
using backend_trial.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace backend_trial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            this.authRepository = authRepository;
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<string>> Register([FromBody] RegisterRequestDto request)
        {
            var (success, message) = await authRepository.RegisterAsync(request);

            if (!success)
            {
                return BadRequest(message);
            }

            return Ok(message);
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            var (success, user, message) = await authRepository.LoginAsync(request);

            if (!success)
            {
                return Unauthorized(message);
            }

            return Ok(user);
        }
    }
}
