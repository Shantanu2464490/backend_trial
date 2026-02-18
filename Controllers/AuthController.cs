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
        public async Task<ActionResult<AuthResponseWrapper>> Register([FromBody] RegisterRequestDto request)
        {
            var (success, message) = await authRepository.RegisterAsync(request);

            if (!success)
            {
                return BadRequest(new AuthResponseWrapper
                {
                    Success = false,
                    Message = message,
                    Data = null
                });
            }

            return Ok(new AuthResponseWrapper
            {
                Success = true,
                Message = message,
                Data = null
            });
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<AuthResponseWrapper>> Login([FromBody] LoginRequestDto request)
        {
            var (success, user, message) = await authRepository.LoginAsync(request);

            if (!success)
            {
                return Unauthorized(new AuthResponseWrapper
                {
                    Success = false,
                    Message = message,
                    Data = null
                });
            }

            return Ok(new AuthResponseWrapper
            {
                Success = true,
                Message = message,
                Data = user
            });
        }
    }
}
