using backend_trial.Data;
using backend_trial.Models.Domain;
using backend_trial.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_trial.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly IdeaBoardDbContext context;

        public CommentRepository(IdeaBoardDbContext context)
        {
            this.context = context;
        }

        public async Task<Comment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CommentId == id, ct);
        }

        public async Task<IEnumerable<Comment>> GetCommentsByIdeaIdAsync(Guid ideaId, CancellationToken ct = default)
        {
            return await context.Comments
                .Where(c => c.IdeaId == ideaId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync(ct);
        }

        public async Task<bool> IdeaExistsAsync(Guid ideaId, CancellationToken ct = default)
        {
            return await context.Ideas.AnyAsync(i => i.IdeaId == ideaId, ct);
        }

        public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.UserId == userId, ct);
        }

        public async Task AddAsync(Comment comment, CancellationToken ct = default)
        {
            await context.Comments.AddAsync(comment, ct);
        }

        public async Task UpdateAsync(Comment comment, CancellationToken ct = default)
        {
            context.Comments.Update(comment);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Comment comment, CancellationToken ct = default)
        {
            context.Comments.Remove(comment);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await context.SaveChangesAsync(ct);
        }
    }
}
