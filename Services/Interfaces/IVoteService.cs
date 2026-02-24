using backend_trial.Models.DTO.Vote;

namespace backend_trial.Services.Interfaces
{
    public interface IVoteService
    {
        Task<UserVoteResponseDto> GetUserVoteStatusAsync(Guid ideaId, Guid currentUserId, CancellationToken ct = default);
        Task<VoteResponseDto> AddUpvoteAsync(Guid ideaId, Guid currentUserId, CancellationToken ct = default);
        Task<VoteResponseDto> AddDownvoteAsync(Guid ideaId, Guid currentUserId, string commentText, CancellationToken ct = default);
        Task RemoveVoteAsync(Guid ideaId, Guid currentUserId, CancellationToken ct = default);
        Task<IEnumerable<VoteResponseDto>> GetVotesForIdeaAsync(Guid ideaId, CancellationToken ct = default);
    }
}