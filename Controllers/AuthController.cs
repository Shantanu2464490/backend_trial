using backend_trial.Models.DTO.Auth;
using backend_trial.Repositories.Interfaces;
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

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, user, message, statusCode) = await authRepository.LoginAsync(request);

            if (!success)
            {
                return StatusCode(statusCode, new AuthResponseWrapper
                {
                    Success = false,
                    Message = message,
                    Data = null,
                    responseCode = statusCode
                });
            }

            return Ok(new AuthResponseWrapper
            {
                Success = true,
                Message = message,
                Data = user,
                responseCode = statusCode
            });
        }
    }
}
