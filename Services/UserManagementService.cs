using backend_trial.Models.Domain;
using backend_trial.Models.DTO.User;
using backend_trial.Repositories.Interfaces;
using backend_trial.Services.Interfaces;

namespace backend_trial.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserManagementRepository userManagementRepository;

        public UserManagementService(IUserManagementRepository userManagementRepository)
        {
            this.userManagementRepository = userManagementRepository;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(CancellationToken ct = default)
        {
            // Retruns all the users(if many user are there we can use pagination)
            var users = await userManagementRepository.GetAllUsersAsync(ct);

            return users.Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role.ToString(),
                Status = u.Status.ToString()
            });
        }

        public async Task<IEnumerable<UserResponseDto>> GetUsersByRoleAsync(string role, CancellationToken ct = default)
        {
            // Returns users based on their role (Employee, Manager, Admin)
            if (!Enum.TryParse<UserRole>(role, true, out var userRole))
            {
                throw new ArgumentException("Invalid role. Valid values are: Employee, Manager, Admin");
            }

            var users = await userManagementRepository.GetUsersByRoleAsync(userRole, ct);

            return users.Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role.ToString(),
                Status = u.Status.ToString()
            });
        }

        public async Task<IEnumerable<UserResponseDto>> GetUsersByStatusAsync(string status, CancellationToken ct = default)
        {
            // Returns users based on their status (Active, Inactive)
            if (!Enum.TryParse<UserStatus>(status, true, out var userStatus))
            {
                throw new ArgumentException("Invalid status. Valid values are: Active, Inactive");
            }

            var users = await userManagementRepository.GetUsersByStatusAsync(userStatus, ct);

            return users.Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role.ToString(),
                Status = u.Status.ToString()
            });
        }

        public async Task<UserDetailResponseDto> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
        {
            // Returns detailed information about a user, including their submitted ideas, comments, votes, and reviews
            var user = await userManagementRepository.GetUserByIdWithDetailsAsync(userId, ct);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            return new UserDetailResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                Status = user.Status.ToString(),
                IdeasSubmitted = user.SubmittedIdeas.Count,
                CommentsPosted = user.Comments.Count,
                VotesCasted = user.Votes.Count,
                ReviewsSubmitted = user.ReviewsAuthored.Count
            };
        }

        public async Task<UserDetailResponseDto> GetUserByEmailAsync(string email, CancellationToken ct = default)
        {
            // Returns detailed information about a user based on their email, including their submitted ideas, comments, votes, and reviews
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be empty");
            }

            var user = await userManagementRepository.GetUserByEmailWithDetailsAsync(email, ct);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            return new UserDetailResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                Status = user.Status.ToString(),
                IdeasSubmitted = user.SubmittedIdeas.Count,
                CommentsPosted = user.Comments.Count,
                VotesCasted = user.Votes.Count,
                ReviewsSubmitted = user.ReviewsAuthored.Count
            };
        }

        public async Task<(string Message, UserResponseDto User)> ToggleUserStatusAsync(Guid userId, string status, Guid currentUserId, CancellationToken ct = default)
        {
            // Toggles a user's status between Active and Inactive. Prevents users from deactivating their own accounts.
            if (!Enum.TryParse<UserStatus>(status, true, out var newStatus))
            {
                throw new ArgumentException("Invalid status. Valid values are: Active, Inactive");
            }
            // Fetch the user from the database
            var user = await userManagementRepository.GetUserByIdAsync(userId, ct);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }
            // Prevent users from deactivating their own accounts
            if (currentUserId == userId && newStatus == UserStatus.Inactive)
            {
                throw new InvalidOperationException("You cannot deactivate your own account");
            }
            // Toggle the user's status
            var oldStatus = user.Status;
            user.Status = newStatus;
            // Update the user in the database
            await userManagementRepository.UpdateAsync(user, ct);
            await userManagementRepository.SaveChangesAsync(ct);

            var response = new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                Status = user.Status.ToString()
            };
            // Return a message indicating the status change along with the updated user information
            return ($"User status changed from {oldStatus} to {newStatus}", response);
        }

        public async Task<UserResponseDto> ActivateUserAsync(Guid userId, CancellationToken ct = default)
        {
            // Activates a user account. Only applicable for users currently marked as Inactive.
            var user = await userManagementRepository.GetUserByIdAsync(userId, ct);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            // Check if the user is already active
            if (user.Status == UserStatus.Active)
            {
                throw new InvalidOperationException("User is already active");
            }
            // Activate the user
            user.Status = UserStatus.Active;
            await userManagementRepository.UpdateAsync(user, ct);
            await userManagementRepository.SaveChangesAsync(ct);

            return new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                Status = user.Status.ToString()
            };
        }

        public async Task<UserResponseDto> DeactivateUserAsync(Guid userId, Guid currentUserId, CancellationToken ct = default)
        {
            // Deactivates a user account. Only applicable for users currently marked as Active. Prevents users from deactivating their own accounts.
            var user = await userManagementRepository.GetUserByIdAsync(userId, ct);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }
            // Prevent users from deactivating their own accounts
            if (currentUserId == userId)
            {
                throw new InvalidOperationException("You cannot deactivate your own account");
            }

            if (user.Status == UserStatus.Inactive)
            {
                throw new InvalidOperationException("User is already inactive");
            }
            // Deactivate the user
            user.Status = UserStatus.Inactive;
            await userManagementRepository.UpdateAsync(user, ct);
            await userManagementRepository.SaveChangesAsync(ct);

            return new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                Status = user.Status.ToString()
            };
        }

        public async Task<(string Message, UserResponseDto User)> UpdateUserRoleAsync(Guid userId, string role, Guid currentUserId, CancellationToken ct = default)
        {
            // Updates a user's role. Prevents users from changing their own roles to avoid privilege escalation.
            if (!Enum.TryParse<UserRole>(role, true, out var newRole))
            {
                throw new ArgumentException("Invalid role. Valid values are: Employee, Manager, Admin");
            }

            var user = await userManagementRepository.GetUserByIdAsync(userId, ct);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }
            // Prevent users from changing their own roles
            if (currentUserId == userId)
            {
                throw new InvalidOperationException("You cannot change your own role");
            }
            // Update the user's role
            var oldRole = user.Role;
            user.Role = newRole;
            // Update the user in the database
            await userManagementRepository.UpdateAsync(user, ct);
            await userManagementRepository.SaveChangesAsync(ct);

            var response = new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                Status = user.Status.ToString()
            };

            return ($"User role changed from {oldRole} to {newRole}", response);
        }

        public async Task<object> GetUserStatisticsAsync(CancellationToken ct = default)
        {
            // Provides statistics about users, such as total number of users, number of active vs. inactive users, and breakdown by role.
            var totalUsers = await userManagementRepository.GetTotalUsersCountAsync(ct);
            var activeUsers = await userManagementRepository.GetActiveUsersCountAsync(ct);
            var inactiveUsers = await userManagementRepository.GetInactiveUsersCountAsync(ct);

            var employeeCount = await userManagementRepository.GetUsersByRoleCountAsync(UserRole.Employee, ct);
            var managerCount = await userManagementRepository.GetUsersByRoleCountAsync(UserRole.Manager, ct);
            var adminCount = await userManagementRepository.GetUsersByRoleCountAsync(UserRole.Admin, ct);

            // Return the statistics in a structured format
            return new
            {
                totalUsers,
                activeUsers,
                inactiveUsers,
                roleBreakdown = new
                {
                    employees = employeeCount,
                    managers = managerCount,
                    admins = adminCount
                }
            };
        }

        public async Task<IEnumerable<UserResponseDto>> SearchUsersAsync(string searchTerm, CancellationToken ct = default)
        {
            // Allows searching for users based on their name or email. Supports partial matches and is case-insensitive.
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentException("Search term cannot be empty");
            }
            // Perform the search using the repository method
            var users = await userManagementRepository.SearchUsersAsync(searchTerm, ct);

            // If no users are found, return an empty list or throw an exception based on your preference
            if (!users.Any())
            {
                throw new KeyNotFoundException("No users found matching the search criteria");
            }

            // Map the results to UserResponseDto and return
            return users.Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role.ToString(),
                Status = u.Status.ToString()
            });
        }
    }
}
