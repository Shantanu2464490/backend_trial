using backend_trial.Models.Domain;

namespace backend_trial.Repositories.Interfaces
{
    public interface IReviewRepository
    {
        Task<List<Idea>> GetAllIdeasForReviewAsync(CancellationToken ct = default);
        Task<List<Idea>> GetIdeasByStatusAsync(IdeaStatus status, CancellationToken ct = default);
        Task<Idea?> GetIdeaForReviewAsync(Guid ideaId, CancellationToken ct = default);

        Task<Review?> GetReviewByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<Review>> GetReviewsForIdeaAsync(Guid ideaId, CancellationToken ct = default);
        Task<List<Review>> GetReviewsByManagerAsync(Guid managerId, CancellationToken ct = default);

        Task<bool> IdeaExistsAsync(Guid ideaId, CancellationToken ct = default);
        Task<Idea?> GetIdeaWithReviewerAsync(Guid ideaId, CancellationToken ct = default);

        Task<User?> GetUserAsync(Guid userId, CancellationToken ct = default);

        Task AddReviewAsync(Review review, CancellationToken ct = default);
        Task UpdateIdeaAsync(Idea idea, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}