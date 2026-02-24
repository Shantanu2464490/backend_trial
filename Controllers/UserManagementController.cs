using backend_trial.Models.DTO;
using backend_trial.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend_trial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementService userManagementService;

        public UserManagementController(IUserManagementService userManagementService)
        {
            this.userManagementService = userManagementService;
        }

        // Get all users
        [HttpGet("users")]
        public async Task<ActionResult> GetAllUsers()
        {
            var users = await userManagementService.GetAllUsersAsync();
            return Ok(users);
        }

        // Get users by role
        [HttpGet("users/role/{role}")]
        public async Task<ActionResult> GetUsersByRole(string role)
        {
            var users = await userManagementService.GetUsersByRoleAsync(role);
            return Ok(users);
        }

        // Get users by status
        [HttpGet("users/status/{status}")]
        public async Task<ActionResult> GetUsersByStatus(string status)
        {
            var users = await userManagementService.GetUsersByStatusAsync(status);
            return Ok(users);
        }

        // Get user by ID with detailed information
        [HttpGet("{userId}")]
        public async Task<ActionResult> GetUserById(Guid userId)
        {
            var userDetail = await userManagementService.GetUserByIdAsync(userId);
            return Ok(userDetail);
        }

        // Get user by email
        [HttpGet("email/{email}")]
        public async Task<ActionResult> GetUserByEmail(string email)
        {
            var userDetail = await userManagementService.GetUserByEmailAsync(email);
            return Ok(userDetail);
        }

        // Toggle user status (Active/Inactive)
        [HttpPut("{userId}/status")]
        public async Task<ActionResult> ToggleUserStatus(Guid userId, [FromBody] ToggleUserStatusRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var userGuid))
            {
                return Unauthorized(new { Message = "User ID not found in token" });
            }

            var (message, user) = await userManagementService.ToggleUserStatusAsync(userId, request.Status, userGuid);

            return Ok(new { Message = message, User = user });
        }

        // Activate user
        [HttpPut("{userId}/activate")]
        public async Task<ActionResult> ActivateUser(Guid userId)
        {
            var response = await userManagementService.ActivateUserAsync(userId);
            return Ok(new { Message = "User activated successfully", User = response });
        }

        // Deactivate user
        [HttpPut("{userId}/deactivate")]
        public async Task<ActionResult> DeactivateUser(Guid userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var userGuid))
            {
                return Unauthorized(new { Message = "User ID not found in token" });
            }

            var response = await userManagementService.DeactivateUserAsync(userId, userGuid);
            return Ok(new { Message = "User deactivated successfully", User = response });
        }

        // Update user role
        [HttpPut("{userId}/role")]
        public async Task<ActionResult> UpdateUserRole(Guid userId, [FromBody] UpdateUserRoleRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var userGuid))
            {
                return Unauthorized(new { Message = "User ID not found in token" });
            }

            var (message, user) = await userManagementService.UpdateUserRoleAsync(userId, request.Role, userGuid);

            return Ok(new { Message = message, User = user });
        }

        // Get statistics of users
        [HttpGet("statistics/summary")]
        public async Task<ActionResult> GetUserStatistics()
        {
            var statistics = await userManagementService.GetUserStatisticsAsync();
            return Ok(statistics);
        }

        // Search users by name
        [HttpGet("search/{searchTerm}")]
        public async Task<ActionResult> SearchUsers(string searchTerm)
        {
            var users = await userManagementService.SearchUsersAsync(searchTerm);
            return Ok(users);
        }
    }
}
