using backend_trial.Data;
using backend_trial.Models.Domain;
using backend_trial.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_trial.Repositories
{
    public class IdeaRepository : IIdeaRepository
    {
        private readonly IdeaBoardDbContext context;

        public IdeaRepository(IdeaBoardDbContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<Idea>> GetAllIdeasAsync(CancellationToken ct = default)
        {
            return await context.Ideas
                .Include(i => i.Category)
                .Include(i => i.SubmittedByUser)
                .Include(i => i.Comments)
                    .ThenInclude(c => c.User)
                .Include(i => i.Votes)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Idea>> GetIdeasByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await context.Ideas
                .Where(i => i.SubmittedByUserId == userId)
                .Include(i => i.Category)
                .Include(i => i.SubmittedByUser)
                .Include(i => i.Comments)
                    .ThenInclude(c => c.User)
                .Include(i => i.Votes)
                .OrderByDescending(i => i.SubmittedDate)
                .ToListAsync(ct);
        }

        public async Task<Idea?> GetIdeaByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await context.Ideas
                .Include(i => i.Category)
                .Include(i => i.SubmittedByUser)
                .Include(i => i.Comments)
                    .ThenInclude(c => c.User)
                .Include(i => i.Votes)
                .Include(i => i.Reviews)
                .FirstOrDefaultAsync(i => i.IdeaId == id, ct);
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid categoryId, CancellationToken ct = default)
        {
            return await context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId, ct);
        }

        public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.UserId == userId, ct);
        }

        public async Task AddAsync(Idea idea, CancellationToken ct = default)
        {
            await context.Ideas.AddAsync(idea, ct);
        }

        public async Task UpdateAsync(Idea idea, CancellationToken ct = default)
        {
            context.Ideas.Update(idea);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Idea idea, CancellationToken ct = default)
        {
            context.Ideas.Remove(idea);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await context.SaveChangesAsync(ct);
        }
    }
}
