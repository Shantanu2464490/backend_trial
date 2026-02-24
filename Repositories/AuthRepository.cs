using backend_trial.Data;
using backend_trial.Models.Domain;
using backend_trial.Models.DTO.Auth;
using backend_trial.Repositories.Interfaces;
using backend_trial.Services.Interfaces;
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

            // Role-specific validation and status assignment
            UserStatus userStatus = UserStatus.Active;

            if (userRole == UserRole.Manager)
            {
                // Managers are inactive by default
                userStatus = UserStatus.Inactive;
            }
            else if (userRole == UserRole.Admin)
            {
                // Check admin registration constraints
                var adminCount = await context.Users.CountAsync(u => u.Role == UserRole.Admin);

                if (adminCount >= 2)
                {
                    return (false, "Admin registration is restricted. Maximum 2 admins are allowed in the system.");
                }

                // First admin is active by default, subsequent admins are inactive
                userStatus = adminCount == 0 ? UserStatus.Active : UserStatus.Inactive;
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
                Status = userStatus
            };

            // Store user in Database
            context.Users.Add(user);
            await context.SaveChangesAsync();

            string successMessage = userRole == UserRole.Manager 
                ? "Registration successful. Your account is inactive. An admin must activate it before you can login."
                : userRole == UserRole.Admin && userStatus == UserStatus.Inactive
                    ? "Registration successful. Your account is inactive. The primary admin must activate it before you can login."
                    : "Registration successful. Please login to continue.";

            return (true, successMessage);
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
                return (false, null, "Your account is not active. Please contact an administrator.");
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
