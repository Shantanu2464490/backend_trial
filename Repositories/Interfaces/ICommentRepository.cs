using backend_trial.Models.Domain;

namespace backend_trial.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task<Comment?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Comment>> GetCommentsByIdeaIdAsync(Guid ideaId, CancellationToken ct = default);
        Task<bool> IdeaExistsAsync(Guid ideaId, CancellationToken ct = default);
        Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
        Task AddAsync(Comment comment, CancellationToken ct = default);
        Task UpdateAsync(Comment comment, CancellationToken ct = default);
        Task DeleteAsync(Comment comment, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
