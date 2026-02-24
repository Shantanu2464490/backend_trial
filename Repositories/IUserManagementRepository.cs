using backend_trial.Models.Domain;

namespace backend_trial.Repositories.Interfaces
{
    public interface IUserManagementRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken ct = default);
        Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role, CancellationToken ct = default);
        Task<IEnumerable<User>> GetUsersByStatusAsync(UserStatus status, CancellationToken ct = default);
        Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
        Task<User?> GetUserByIdWithDetailsAsync(Guid userId, CancellationToken ct = default);
        Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default);
        Task<User?> GetUserByEmailWithDetailsAsync(string email, CancellationToken ct = default);
        Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, CancellationToken ct = default);
        Task<int> GetTotalUsersCountAsync(CancellationToken ct = default);
        Task<int> GetActiveUsersCountAsync(CancellationToken ct = default);
        Task<int> GetInactiveUsersCountAsync(CancellationToken ct = default);
        Task<int> GetUsersByRoleCountAsync(UserRole role, CancellationToken ct = default);
        Task UpdateAsync(User user, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
