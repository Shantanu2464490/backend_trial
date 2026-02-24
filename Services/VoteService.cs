// Services/VoteService.cs
using backend_trial.Models.Domain;
using backend_trial.Models.DTO;
using backend_trial.Repositories.Interfaces;
using backend_trial.Services.Interfaces;

namespace backend_trial.Services
{
    public class VoteService : IVoteService
    {
        private readonly IVoteRepository voteRepository;

        public VoteService(IVoteRepository voteRepository)
        {
            this.voteRepository = voteRepository;
        }

        public async Task<UserVoteResponseDto> GetUserVoteStatusAsync(Guid ideaId, Guid currentUserId, CancellationToken ct = default)
        {
            await EnsureIdeaExists(ideaId, ct);

            var existing = await voteRepository.GetUserVoteAsync(currentUserId, ideaId, ct);

            return new UserVoteResponseDto
            {
                HasVoted = existing != null,
                VoteType = existing?.VoteType.ToString()
            };
        }

        public async Task<VoteResponseDto> AddUpvoteAsync(Guid ideaId, Guid currentUserId, CancellationToken ct = default)
        {
            await EnsureIdeaExists(ideaId, ct);
            var user = await EnsureUserExists(currentUserId, ct);

            var existing = await voteRepository.GetUserVoteAsync(currentUserId, ideaId, ct);
            if (existing != null)
            {
                if (existing.VoteType == VoteType.Upvote)
                    throw new ("You have already upvoted this idea");

                // Convert downvote → upvote and remove the latest comment for this idea/user (matching your original behavior)
                var latestComment = await voteRepository.GetLatestCommentForUserIdeaAsync(currentUserId, ideaId, ct);
                if (latestComment != null)
                {
                    await voteRepository.RemoveCommentAsync(latestComment, ct);
                }

                existing.VoteType = VoteType.Upvote;
                await voteRepository.UpdateVoteAsync(existing, ct);
            }
            else
            {
                var vote = new Vote
                {
                    VoteId = Guid.NewGuid(),
                    IdeaId = ideaId,
                    UserId = currentUserId,
                    VoteType = VoteType.Upvote
                };
                await voteRepository.AddVoteAsync(vote, ct);
            }

            await voteRepository.SaveChangesAsync(ct);

            var final = await voteRepository.GetUserVoteAsync(currentUserId, ideaId, ct)!;

            return new VoteResponseDto
            {
                VoteId = final!.VoteId,
                IdeaId = final.IdeaId,
                UserId = final.UserId,
                UserName = user.Name ?? "Unknown",
                VoteType = final.VoteType.ToString()
            };
        }

        public async Task<VoteResponseDto> AddDownvoteAsync(Guid ideaId, Guid currentUserId, string commentText, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(commentText))
                throw new ("Comment is mandatory when downvoting. Please provide a reason for your downvote.");

            await EnsureIdeaExists(ideaId, ct);
            var user = await EnsureUserExists(currentUserId, ct);

            var existing = await voteRepository.GetUserVoteAsync(currentUserId, ideaId, ct);

            if (existing != null)
            {
                if (existing.VoteType == VoteType.Downvote)
                    throw new ("You have already downvoted this idea");

                // Convert upvote → downvote
                existing.VoteType = VoteType.Downvote;
                await voteRepository.UpdateVoteAsync(existing, ct);
            }
            else
            {
                var vote = new Vote
                {
                    VoteId = Guid.NewGuid(),
                    IdeaId = ideaId,
                    UserId = currentUserId,
                    VoteType = VoteType.Downvote
                };
                await voteRepository.AddVoteAsync(vote, ct);
            }

            // Add mandatory comment for downvote
            var comment = new Comment
            {
                CommentId = Guid.NewGuid(),
                IdeaId = ideaId,
                UserId = currentUserId,
                Text = commentText,
                CreatedDate = DateTime.UtcNow
            };
            await voteRepository.AddCommentAsync(comment, ct);

            await voteRepository.SaveChangesAsync(ct);

            var final = await voteRepository.GetUserVoteAsync(currentUserId, ideaId, ct)!;

            return new VoteResponseDto
            {
                VoteId = final!.VoteId,
                IdeaId = final.IdeaId,
                UserId = final.UserId,
                UserName = user.Name,
                VoteType = final.VoteType.ToString()
            };
        }

        public async Task RemoveVoteAsync(Guid ideaId, Guid currentUserId, CancellationToken ct = default)
        {
            await EnsureIdeaExists(ideaId, ct);

            var vote = await voteRepository.GetUserVoteAsync(currentUserId, ideaId, ct);
            if (vote == null)
                throw new ("Vote not found");

            if (vote.VoteType == VoteType.Downvote)
            {
                var latestComment = await voteRepository.GetLatestCommentForUserIdeaAsync(currentUserId, ideaId, ct);
                if (latestComment != null)
                    await voteRepository.RemoveCommentAsync(latestComment, ct);
            }

            await voteRepository.RemoveVoteAsync(vote, ct);
            await voteRepository.SaveChangesAsync(ct);
        }

        public async Task<IEnumerable<VoteResponseDto>> GetVotesForIdeaAsync(Guid ideaId, CancellationToken ct = default)
        {
            var tuples = await voteRepository.GetVotesForIdeaWithUserAsync(ideaId, ct);
            return tuples.Select(t => new VoteResponseDto
            {
                VoteId = t.vote.VoteId,
                IdeaId = t.vote.IdeaId,
                UserId = t.vote.UserId,
                UserName = t.userName,
                VoteType = t.vote.VoteType.ToString()
            });
        }

        private async Task EnsureIdeaExists(Guid ideaId, CancellationToken ct)
        {
            if (!await voteRepository.IdeaExistsAsync(ideaId, ct))
                throw new("Idea not found");
        }

        private async Task<backend_trial.Models.Domain.User> EnsureUserExists(Guid userId, CancellationToken ct)
        {
            var user = await voteRepository.GetUserAsync(userId, ct);
            if (user == null)
                throw new("User not found");
            return user;
        }
    }
}