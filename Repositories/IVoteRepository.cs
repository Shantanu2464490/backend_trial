
using backend_trial.Models.Domain;

namespace backend_trial.Repositories.Interfaces
{
    public interface IVoteRepository
    {
        Task<bool> IdeaExistsAsync(Guid ideaId, CancellationToken ct = default);
        Task<User?> GetUserAsync(Guid userId, CancellationToken ct = default);

        Task<Vote?> GetUserVoteAsync(Guid userId, Guid ideaId, CancellationToken ct = default);
        Task AddVoteAsync(Vote vote, CancellationToken ct = default);
        Task UpdateVoteAsync(Vote vote, CancellationToken ct = default);
        Task RemoveVoteAsync(Vote vote, CancellationToken ct = default);

        Task<Comment?> GetLatestCommentForUserIdeaAsync(Guid userId, Guid ideaId, CancellationToken ct = default);
        Task AddCommentAsync(Comment comment, CancellationToken ct = default);
        Task RemoveCommentAsync(Comment comment, CancellationToken ct = default);

        Task<List<(Vote vote, string userName)>> GetVotesForIdeaWithUserAsync(Guid ideaId, CancellationToken ct = default);

        Task SaveChangesAsync(CancellationToken ct = default);
    }
}