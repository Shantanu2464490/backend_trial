using backend_trial.Data;
using backend_trial.Models.Domain;
using backend_trial.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_trial.Repositories
{
    public class VoteRepository : IVoteRepository
    {
        private readonly IdeaBoardDbContext context;

        public VoteRepository(IdeaBoardDbContext context)
        {
            this.context = context;
        }

        public Task<bool> IdeaExistsAsync(Guid ideaId, CancellationToken ct = default)
        {
            return context.Ideas.AsNoTracking().AnyAsync(i => i.IdeaId == ideaId, ct);
        }

        public Task<User?> GetUserAsync(Guid userId, CancellationToken ct = default)
        {
            return context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId, ct);
        }

        public Task<Vote?> GetUserVoteAsync(Guid userId, Guid ideaId, CancellationToken ct = default)
        {
            return context.Votes.FirstOrDefaultAsync(v => v.UserId == userId && v.IdeaId == ideaId, ct);
        }

        public async Task AddVoteAsync(Vote vote, CancellationToken ct = default)
        {
            await context.Votes.AddAsync(vote, ct);
        }

        public Task UpdateVoteAsync(Vote vote, CancellationToken ct = default)
        {
            context.Votes.Update(vote);
            return Task.CompletedTask;
        }

        public Task RemoveVoteAsync(Vote vote, CancellationToken ct = default)
        {
            context.Votes.Remove(vote);
            return Task.CompletedTask;
        }

        public Task<Comment?> GetLatestCommentForUserIdeaAsync(Guid userId, Guid ideaId, CancellationToken ct = default)
        {
            return context.Comments
                .Where(c => c.UserId == userId && c.IdeaId == ideaId)
                .OrderByDescending(c => c.CreatedDate)
                .FirstOrDefaultAsync(ct);
        }

        public async Task AddCommentAsync(Comment comment, CancellationToken ct = default)
        {
            await context.Comments.AddAsync(comment, ct);
        }

        public Task RemoveCommentAsync(Comment comment, CancellationToken ct = default)
        {
            context.Comments.Remove(comment);
            return Task.CompletedTask;
        }

        public async Task<List<(Vote vote, string userName)>> GetVotesForIdeaWithUserAsync(Guid ideaId, CancellationToken ct = default)
        {
            var list = await context.Votes.AsNoTracking()
                .Where(v => v.IdeaId == ideaId)
                .Include(v => v.User)
                .ToListAsync(ct);

            return list.Select(v => (v, v.User?.Name ?? "Unknown")).ToList();
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            return context.SaveChangesAsync(ct);
        }
    }
}