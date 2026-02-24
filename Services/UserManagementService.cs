using backend_trial.Models.Domain;
using backend_trial.Models.DTO;
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
            if (!Enum.TryParse<UserStatus>(status, true, out var newStatus))
            {
                throw new ArgumentException("Invalid status. Valid values are: Active, Inactive");
            }

            var user = await userManagementRepository.GetUserByIdAsync(userId, ct);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (currentUserId == userId && newStatus == UserStatus.Inactive)
            {
                throw new InvalidOperationException("You cannot deactivate your own account");
            }

            var oldStatus = user.Status;
            user.Status = newStatus;

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

            return ($"User status changed from {oldStatus} to {newStatus}", response);
        }

        public async Task<UserResponseDto> ActivateUserAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await userManagementRepository.GetUserByIdAsync(userId, ct);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (user.Status == UserStatus.Active)
            {
                throw new InvalidOperationException("User is already active");
            }

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
            var user = await userManagementRepository.GetUserByIdAsync(userId, ct);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (currentUserId == userId)
            {
                throw new InvalidOperationException("You cannot deactivate your own account");
            }

            if (user.Status == UserStatus.Inactive)
            {
                throw new InvalidOperationException("User is already inactive");
            }

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
            if (!Enum.TryParse<UserRole>(role, true, out var newRole))
            {
                throw new ArgumentException("Invalid role. Valid values are: Employee, Manager, Admin");
            }

            var user = await userManagementRepository.GetUserByIdAsync(userId, ct);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (currentUserId == userId)
            {
                throw new InvalidOperationException("You cannot change your own role");
            }

            var oldRole = user.Role;
            user.Role = newRole;

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
            var totalUsers = await userManagementRepository.GetTotalUsersCountAsync(ct);
            var activeUsers = await userManagementRepository.GetActiveUsersCountAsync(ct);
            var inactiveUsers = await userManagementRepository.GetInactiveUsersCountAsync(ct);

            var employeeCount = await userManagementRepository.GetUsersByRoleCountAsync(UserRole.Employee, ct);
            var managerCount = await userManagementRepository.GetUsersByRoleCountAsync(UserRole.Manager, ct);
            var adminCount = await userManagementRepository.GetUsersByRoleCountAsync(UserRole.Admin, ct);

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
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentException("Search term cannot be empty");
            }

            var users = await userManagementRepository.SearchUsersAsync(searchTerm, ct);

            if (!users.Any())
            {
                throw new KeyNotFoundException("No users found matching the search criteria");
            }

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
