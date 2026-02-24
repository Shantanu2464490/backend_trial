using backend_trial.Data;
using backend_trial.Models.Domain;
using backend_trial.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_trial.Repositories
{
    public class UserManagementRepository : IUserManagementRepository
    {
        private readonly IdeaBoardDbContext context;

        public UserManagementRepository(IdeaBoardDbContext dbContext)
        {
            context = dbContext;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken ct = default)
        {
            return await context.Users
                .OrderBy(u => u.Name)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role, CancellationToken ct = default)
        {
            return await context.Users
                .Where(u => u.Role == role)
                .OrderBy(u => u.Name)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<User>> GetUsersByStatusAsync(UserStatus status, CancellationToken ct = default)
        {
            return await context.Users
                .Where(u => u.Status == status)
                .OrderBy(u => u.Name)
                .ToListAsync(ct);
        }

        public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId, ct);
        }

        public async Task<User?> GetUserByIdWithDetailsAsync(Guid userId, CancellationToken ct = default)
        {
            return await context.Users
                .Where(u => u.UserId == userId)
                .Include(u => u.SubmittedIdeas)
                .Include(u => u.Comments)
                .Include(u => u.Votes)
                .Include(u => u.ReviewsAuthored)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default)
        {
            return await context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), ct);
        }

        public async Task<User?> GetUserByEmailWithDetailsAsync(string email, CancellationToken ct = default)
        {
            return await context.Users
                .Where(u => u.Email.ToLower() == email.ToLower())
                .Include(u => u.SubmittedIdeas)
                .Include(u => u.Comments)
                .Include(u => u.Votes)
                .Include(u => u.ReviewsAuthored)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, CancellationToken ct = default)
        {
            return await context.Users
                .Where(u => u.Name.ToLower().Contains(searchTerm.ToLower()) ||
                            u.Email.ToLower().Contains(searchTerm.ToLower()))
                .OrderBy(u => u.Name)
                .ToListAsync(ct);
        }

        public async Task<int> GetTotalUsersCountAsync(CancellationToken ct = default)
        {
            return await context.Users.CountAsync(ct);
        }

        public async Task<int> GetActiveUsersCountAsync(CancellationToken ct = default)
        {
            return await context.Users.CountAsync(u => u.Status == UserStatus.Active, ct);
        }

        public async Task<int> GetInactiveUsersCountAsync(CancellationToken ct = default)
        {
            return await context.Users.CountAsync(u => u.Status == UserStatus.Inactive, ct);
        }

        public async Task<int> GetUsersByRoleCountAsync(UserRole role, CancellationToken ct = default)
        {
            return await context.Users.CountAsync(u => u.Role == role, ct);
        }

        public async Task UpdateAsync(User user, CancellationToken ct = default)
        {
            context.Users.Update(user);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await context.SaveChangesAsync(ct);
        }
    }
}
