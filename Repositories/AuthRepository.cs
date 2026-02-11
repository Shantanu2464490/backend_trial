using backend_trial.Data;
using backend_trial.Models.Domain;
using backend_trial.Models.DTO;
using backend_trial.Services;
using Microsoft.EntityFrameworkCore;

namespace backend_trial.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IdeaBoardDbContext context;
        private readonly ITokenService tokenService;

        public AuthRepository(IdeaBoardDbContext context, ITokenService tokenService)
        {
            this.context = context;
            this.tokenService = tokenService;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto request)
        {
            // Check if user with the same email already exists
            if (await UserExistsAsync(request.Email))
            {
                return (false, "User with this email already exists.");
            }

            // Parse role string to enum
            if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
            {
                return (false, "Invalid role. Must be 'Employee', 'Manager', or 'Admin'.");
            }

            // Hash the password before storing
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create new User
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = userRole,
                Status = UserStatus.Active
            };

            // Store user in Database
            context.Users.Add(user);
            await context.SaveChangesAsync();

            return (true, "Registration successful. Please login to continue.");
        }

        public async Task<(bool Success, AuthResponseDto? User, string Message)> LoginAsync(LoginRequestDto request)
        {
            // Find user by email
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return (false, null, "Invalid email or password.");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return (false, null, "Invalid email or password.");
            }

            // Check if user is active
            if (user.Status != UserStatus.Active)
            {
                return (false, null, "User account is not active.");
            }

            // Generate Jwt token
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

            return (true, response, "Login successful.");
        }
    }
}
