using backend_trial.Data;
using backend_trial.Models.Domain;
using backend_trial.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_trial.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly IdeaBoardDbContext context;

        public ReviewRepository(IdeaBoardDbContext context)
        {
            this.context = context;
        }

        public Task<List<Idea>> GetAllIdeasForReviewAsync(CancellationToken ct = default)
        {
            return context.Ideas
                .Include(i => i.Category)
                .Include(i => i.SubmittedByUser)
                .Include(i => i.Votes)
                .Include(i => i.Comments).ThenInclude(c => c.User)
                .Include(i => i.Reviews).ThenInclude(r => r.Reviewer)
                .OrderByDescending(i => i.SubmittedDate)
                .ToListAsync(ct);
        }

        public Task<List<Idea>> GetIdeasByStatusAsync(IdeaStatus status, CancellationToken ct = default)
        {
            return context.Ideas
                .Where(i => i.Status == status)
                .Include(i => i.Category)
                .Include(i => i.SubmittedByUser)
                .Include(i => i.Votes)
                .Include(i => i.Comments).ThenInclude(c => c.User)
                .Include(i => i.Reviews).ThenInclude(r => r.Reviewer)
                .OrderByDescending(i => i.SubmittedDate)
                .ToListAsync(ct);
        }

        public Task<Idea?> GetIdeaForReviewAsync(Guid ideaId, CancellationToken ct = default)
        {
            return context.Ideas
                .Include(i => i.Category)
                .Include(i => i.SubmittedByUser)
                .Include(i => i.Votes)
                .Include(i => i.Comments).ThenInclude(c => c.User)
                .Include(i => i.Reviews).ThenInclude(r => r.Reviewer)
                .FirstOrDefaultAsync(i => i.IdeaId == ideaId, ct);
        }

        public Task<bool> IdeaExistsAsync(Guid ideaId, CancellationToken ct = default)
        {
            return context.Ideas.AnyAsync(i => i.IdeaId == ideaId, ct);
        }

        public Task<Idea?> GetIdeaWithReviewerAsync(Guid ideaId, CancellationToken ct = default)
        {
            return context.Ideas.Include(i => i.SubmittedByUser)
                .Include(i => i.Reviews)
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.IdeaId == ideaId, ct);
        }

        public Task<User?> GetUserAsync(Guid userId, CancellationToken ct = default)
        {
            return context.Users.FirstOrDefaultAsync(u => u.UserId == userId, ct);
        }

        public Task<Review?> GetReviewByIdAsync(Guid id, CancellationToken ct = default)
        {
            return context.Reviews
                .Include(r => r.Reviewer)
                .FirstOrDefaultAsync(r => r.ReviewId == id, ct);
        }

        public Task<List<Review>> GetReviewsForIdeaAsync(Guid ideaId, CancellationToken ct = default)
        {
            return context.Reviews
                .Where(r => r.IdeaId == ideaId)
                .Include(r => r.Reviewer)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync(ct);
        }

        public Task<List<Review>> GetReviewsByManagerAsync(Guid managerId, CancellationToken ct = default)
        {
            return context.Reviews
                .Where(r => r.ReviewerId == managerId)
                .Include(r => r.Reviewer)
                .Include(r => r.Idea)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync(ct);
        }

        public async Task AddReviewAsync(Review review, CancellationToken ct = default)
        {
            await context.Reviews.AddAsync(review, ct);
        }

        public Task UpdateIdeaAsync(Idea idea, CancellationToken ct = default)
        {
            context.Ideas.Update(idea);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            return context.SaveChangesAsync(ct);
        }
    }
}