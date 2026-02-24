using backend_trial.Models.DTO;
using backend_trial.Models.Domain;

namespace backend_trial.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(CancellationToken ct = default);
        Task<IEnumerable<UserResponseDto>> GetUsersByRoleAsync(string role, CancellationToken ct = default);
        Task<IEnumerable<UserResponseDto>> GetUsersByStatusAsync(string status, CancellationToken ct = default);
        Task<UserDetailResponseDto> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
        Task<UserDetailResponseDto> GetUserByEmailAsync(string email, CancellationToken ct = default);
        Task<(string Message, UserResponseDto User)> ToggleUserStatusAsync(Guid userId, string status, Guid currentUserId, CancellationToken ct = default);
        Task<UserResponseDto> ActivateUserAsync(Guid userId, CancellationToken ct = default);
        Task<UserResponseDto> DeactivateUserAsync(Guid userId, Guid currentUserId, CancellationToken ct = default);
        Task<(string Message, UserResponseDto User)> UpdateUserRoleAsync(Guid userId, string role, Guid currentUserId, CancellationToken ct = default);
        Task<object> GetUserStatisticsAsync(CancellationToken ct = default);
        Task<IEnumerable<UserResponseDto>> SearchUsersAsync(string searchTerm, CancellationToken ct = default);
    }
}
