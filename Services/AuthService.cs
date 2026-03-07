// Services/AuthService.cs
using backend_trial.Models.Domain;
using backend_trial.Models.DTO.Auth;
using backend_trial.Repositories.Interfaces;
using backend_trial.Services.Interfaces;

namespace backend_trial.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository authRepository;
        private readonly ITokenService tokenService;

        public AuthService(IAuthRepository authRepository, ITokenService tokenService)
        {
            this.authRepository = authRepository;
            this.tokenService = tokenService;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto request)
        {
            //  Check duplicate email
            var exists = await authRepository.UserExistsAsync(request.Email);
            if (exists)
                return (false, "User with this email already exists.");

            // Validate role
            if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
                return (false, "Invalid role. Must be 'Employee', 'Manager', or 'Admin'.");

            // Decide status per role (business rule)
            var status = UserStatus.Active;

            if (role == UserRole.Manager)
            {
                status = UserStatus.Inactive;
            }
            else if (role == UserRole.Admin)
            {
                // Admin cap logic delegated to repository to avoid races if you want,
                // but we can keep the count retrieval here and let repo throw if unique constraints exist.
                var adminCount = await authRepository.CountAdminsAsync();
                if (adminCount >= 2)
                    return (false, "Admin registration is restricted. Maximum 2 admins are allowed in the system.");

                status = adminCount == 0 ? UserStatus.Active : UserStatus.Inactive;
            }

            //  Hash password (business concern; repo shouldn't do crypto)
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Build domain model (service decides values)
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = role,
                Status = status
            };

            //  Persist via repository (pure data operation)
            await authRepository.CreateUserAsync(user);

            //  Business message
            string successMessage =
                role == UserRole.Manager
                    ? "Registration successful. Your account is inactive. An admin must activate it before you can login."
                : (role == UserRole.Admin && status == UserStatus.Inactive)
                    ? "Registration successful. Your account is inactive. The primary admin must activate it before you can login."
                : "Registration successful. Please login to continue.";

            return (true, successMessage);
        }

        public async Task<(bool Success, AuthResponseDto? User, string Message, int statusCode)> LoginAsync(LoginRequestDto request)
        {
            //  Lookup user
            var user = await authRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
                return (false, null, "Invalid email or password.", 401);

            //  Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return (false, null, "Invalid email or password.", 401);

            //  Check status
            if (user.Status != UserStatus.Active)
                return (false, null, "Your account is not active. Please contact an administrator.", 403);

            //  Issue token (business concern)
            var token = tokenService.CreateJwtToken(user);

            var response = new AuthResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Status = user.Status,
                Token = token
            };

            return (true, response, "Login successful.", 200);
        }
    }
}